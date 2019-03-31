using System;
using System.IO;
using System.IO.Compression;

namespace LogPacker {
    public class LogCompressor {
        private const byte ESCAPE = 0x7F;
        private const byte NEWLINE_DELIMITER = 0xA;
        private const byte CARRIAGE_RETURN = 0xD;
        private const byte INITIAL_LENGTH = 128;
        private const int BUFFER_SIZE = 8192;

        public void Compress(FileStream inputStream, FileStream outputStream) {
            using (var gZipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
            using (var compressed = new BufferedStream(gZipStream, BUFFER_SIZE)) {
                var blocksAmount = inputStream.Length / BUFFER_SIZE;
                if (inputStream.Length % BUFFER_SIZE != 0) blocksAmount++;
                var buffer = new byte[BUFFER_SIZE];
                var currentLine = new byte[BUFFER_SIZE * 5];
                var output = new byte[BUFFER_SIZE * 5];
                var lastLine = (byte[]) null;
                int bytesRead, currentLineCount = 0, lastLineCount = 0, blocks = 0;
                while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0) {
                    blocks++;
                    for (var i = 0; i < bytesRead; i++) {
                        var b = buffer[i];
                        currentLine[currentLineCount++] = b;
                        if (b == NEWLINE_DELIMITER || i == bytesRead - 1 && blocks == blocksAmount) {
                            if (lastLine == null) {
                                lastLine = new byte[BUFFER_SIZE * 5];
                                Array.Copy(currentLine, lastLine, currentLineCount);
                                compressed.Write(currentLine, 0, currentLineCount);
                            }
                            else {
                                byte[] maxString;
                                int minStringLength, maxStringLength;
                                if (lastLineCount > currentLineCount) {
                                    maxString = lastLine;
                                    minStringLength = currentLineCount;
                                    maxStringLength = lastLineCount;
                                }
                                else {
                                    maxString = currentLine;
                                    minStringLength = lastLineCount;
                                    maxStringLength = currentLineCount;
                                }

                                int length = 0, outputIndex = 0;
                                byte lastByte = 0;
                                for (var j = 0; j < minStringLength; j++) {
                                    if (lastLine[j] == currentLine[j] && currentLine[j] != NEWLINE_DELIMITER &&
                                        currentLine[j] != CARRIAGE_RETURN) {
                                        length++;
                                        lastByte = currentLine[j];
                                    }
                                    else {
                                        if (length > 1) {
                                            output[outputIndex++] = ESCAPE;
                                            output[outputIndex++] = Convert.ToByte(INITIAL_LENGTH + length);
                                            length = 0;
                                        }
                                        else if (length == 1) {
                                            if (lastByte == ESCAPE) {
                                                output[outputIndex++] = ESCAPE;
                                            }

                                            output[outputIndex++] = lastByte;
                                            length = 0;
                                        }

                                        if (currentLine[j] == ESCAPE) {
                                            output[outputIndex++] = ESCAPE;
                                        }

                                        output[outputIndex++] = currentLine[j];
                                    }
                                }

                                if (length > 1) {
                                    output[outputIndex++] = ESCAPE;
                                    output[outputIndex++] = Convert.ToByte(INITIAL_LENGTH + length);
                                }
                                else if (length == 1) {
                                    if (lastByte == ESCAPE) {
                                        output[outputIndex++] = ESCAPE;
                                    }

                                    output[outputIndex++] = lastByte;
                                }

                                if (maxString == currentLine) {
                                    for (var j = minStringLength; j < maxStringLength; j++) {
                                        if (maxString[j] == ESCAPE) {
                                            output[outputIndex++] = ESCAPE;
                                        }

                                        output[outputIndex++] = maxString[j];
                                    }
                                }

                                compressed.Write(output, 0, outputIndex);
                                Array.Copy(currentLine, lastLine, currentLineCount);
                            }

                            lastLineCount = currentLineCount;
                            currentLineCount = 0;
                        }
                    }
                }
            }
        }

        public void Decompress(FileStream inputStream, FileStream outputStream) {
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var decompressed = new BufferedStream(gzipStream, BUFFER_SIZE)) {
                var buffer = new byte[BUFFER_SIZE];
                var lastLine = (byte[]) null;
                var currentLine = new byte[BUFFER_SIZE * 5];
                var output = new byte[BUFFER_SIZE * 5];
                int bytesRead, currentLineCount = 0;
                while ((bytesRead = decompressed.Read(buffer, 0, buffer.Length)) != 0) {
                    for (var i = 0; i < bytesRead; i++) {
                        var b = buffer[i];
                        currentLine[currentLineCount++] = b;
                        if (b == NEWLINE_DELIMITER || i == bytesRead - 1 && bytesRead < BUFFER_SIZE) {
                            if (lastLine == null) {
                                lastLine = new byte[BUFFER_SIZE * 5];
                                Array.Copy(currentLine, lastLine, currentLineCount);
                                outputStream.Write(currentLine, 0, currentLineCount);
                            }
                            else {
                                var escaped = false;
                                int outputIndex = 0, lastLineIndex = 0;
                                for (var j = 0; j < currentLineCount; j++) {
                                    var currentByte = currentLine[j];
                                    if (currentByte == ESCAPE) {
                                        if (escaped) {
                                            output[outputIndex++] = currentByte;
                                            lastLineIndex++;
                                        }

                                        escaped = !escaped;
                                        continue;
                                    }

                                    if (currentByte > INITIAL_LENGTH && escaped) {
                                        escaped = false;
                                        var length = currentByte - INITIAL_LENGTH;
                                        for (var k = 0; k < length; k++) {
                                            output[outputIndex++] = lastLine[lastLineIndex + k];
                                        }

                                        lastLineIndex += length;
                                    }
                                    else {
                                        output[outputIndex++] = currentByte;
                                        lastLineIndex++;
                                    }
                                }

                                outputStream.Write(output, 0, outputIndex);
                                Array.Copy(output, lastLine, outputIndex);
                            }

                            currentLineCount = 0;
                        }
                    }
                }
            }
        }
    }
}