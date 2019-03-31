# LogPacker

This is a program that archives logs/random binary data. It uses custom algorithm to prepare data for compressing and then <a href=gzip.org>GZip</a> (.NET standard compression algorithm).

### Sample input
```
2018-11-13 00:02:41,387 888    INFO  [dfad50] Started processing user request. Id = dfad50dd-17ad-4ad3-b75b-c992ee1f1f1a. Size = 37.68 KB.
2018-11-13 00:02:41,394 895    INFO  [dfad50] Doing some complicated stuff.. Random numbers are: 551704423, 1141562255, 1240276331.
2018-11-13 00:02:41,399 900    INFO  [3a61cc] Finished processing user request with id = 3a61cc3f-2ee9-4738-8283-efc073235ec8 in 00:00:00.0536137.
2018-11-13 00:02:41,431 932    INFO  [8c2d7d] Started processing user request. Id = 8c2d7d0d-ea36-45a2-b915-b9b188139594. Size = 73.61 KB.
2018-11-13 00:02:41,435 936    INFO  [8c2d7d] Doing some complicated stuff.. Random numbers are: 1068253945, 1648598399, 436946751.
2018-11-13 00:02:41,442 943    INFO  [dfad50] Finished processing user request with id = dfad50dd-17ad-4ad3-b75b-c992ee1f1f1a in 00:00:00.0550134.
2018-11-13 00:02:41,455 956    INFO  [8afb16] Started processing user request. Id = 8afb16cb-d7e1-41ba-be7b-9b3b1e09416a. Size = 96.65 KB.
2018-11-13 00:02:41,455 956    INFO  [8c2d7d] Here's some useful guids:
		81d80c1e-cfdb-49e2-a820-1e157929dd9c
		600e50f4-c902-4d75-9f09-d48c8d0191b8
		a07133b8-f3bb-4334-9cae-e03594918b2f
2018-11-13 00:02:41,459 960    INFO  [BackgroundDaemon] Processing data (44% completed)..
2018-11-13 00:02:41,470 971    ERROR [8c2d7d] 
System.InvalidOperationException: Oh no! An error occurred! 
```

### Algorithm
+ Read input data line by line
+ Compare adjacent lines
+ If we have some similar substrings, we write down only it's length, encoded in a single byte (128 + length)
+ If we have some random binary data in the input then we just have to <a href=https://en.wikipedia.org/wiki/Consistent_Overhead_Byte_Stuffing>escape</a> our encoded lengths

### Sample output before GZip compression
```
2018-11-13 00:02:41,387 888    INFO  [dfad50] Started processing user request. Id = dfad50dd-17ad-4ad3-b75b-c992ee1f1f1a. Size = 37.68 KB.
(149)94 895(147)Doing some complicated stuff.. Random numbers are: 551704423, 1141562255, 1240276331.
(149)99 900(139)3a61cc(130)Finished processing user request with id = 3a61cc3f-2ee9-4738-8283-efc073235ec8 in 00:00:00.0536137.
...
System.InvalidOperationException: Oh no! An error occurred! 
```

### See also
For more interesting algorithms see <a href=http://www.adbis.org/docs/lp/6.pdf>this website</a>
