EXTERNAL TriggerAnim(key)
EXTERNAL HasMemory(key)

== policeknockdoor ==

~ temp hasMemory = HasMemory("Loop_Proof")
{
    -hasMemory:
    -> FirstAsk
}
-> NoLoopFirstAsk

== NoLoopFirstAsk ==
~ TriggerAnim("onknockdoor")
(敲門聲) 您好，我是警察請你快點開門
...#delay=3
難道有人幫我們報警?
...#delay=2
-> NoLoopSecondAsk

== NoLoopSecondAsk ==
~ TriggerAnim("onknockdoor")
(急促地敲門聲) 請快點開門!
我知道你們在家。
...#delay=2
-> NoLoopThirdAsk

== NoLoopThirdAsk ==
再不開門你們就死定了!
...#delay=2
~ TriggerAnim("onkickdoor")
...#delay=2
~ TriggerAnim("onkickdoor")
...#delay=3
~ TriggerAnim("onkickdoor")
-> END


== FirstAsk ==
~ TriggerAnim("onknockdoor")
(敲門聲) 您好，我是警察請你快點開門
...#delay=2
-> SecondAsk

== SecondAsk ==
~ TriggerAnim("onknockdoor")
(急促地敲門聲) 請快點開門!
我知道你們在家。
...#delay=2
-> ThirdAsk

== ThirdAsk ==
再不開門你們就死定了!
...#delay=2
~ TriggerAnim("onkickdoor")
...#delay=2
~ TriggerAnim("onkickdoor")
...#delay=3
~ TriggerAnim("onkickdoor")
-> END

== Interrupt ==
hi
-> END