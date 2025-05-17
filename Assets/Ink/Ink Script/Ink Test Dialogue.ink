-> NPC

=== NPC ===
怎麼了嗎？
* [為什麼會輪迴？]
-> MainChoice1
* [這裡發生什麼事？]
-> MainChoice2
* [沒有事情]
-> MainChoice3

=== MainChoice1 ===
我為什麼會在這段時間裡一直輪迴？？
-> NPCRespondLooping
=== MainChoice2 ===
這裡是發生什麼事嗎？
-> NPCRespondWhatHappend
=== MainChoice3 ===
沒事，祝你平安。
-> NPCRespondNothing

=== NPCRespondLooping ===
什麼輪迴？？沒有啊!!
-> NPC
=== NPCRespondWhatHappend ===
這裡一直很平靜沒有發生過任何事。
-> NPC
=== NPCRespondNothing ===
好的謝謝！
-> END