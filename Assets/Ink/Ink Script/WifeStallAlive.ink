EXTERNAL Audio(character, audioId)
EXTERNAL HasMemory(key)
=== WifeNeedHelp ===
快..救我...

~ temp hasMemory = HasMemory("SaveWife")
{
    -hasMemory:
    -> FirstLoopNoticedResponse
}
-> FirstNoticedResponse

=== FirstNoticedResponse ===
等我一下我馬上救你!
急救箱在哪裡?
-> END

=== FirstLoopNoticedResponse ===
...不會吧
剛剛妻子說過這句話..
難道真的是我輪迴了嗎?
先冷靜現在最主要的是救起妻子
等我一下我馬上救你!
急救箱在哪裡?
-> END