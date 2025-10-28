EXTERNAL TriggerAnim(key)

== policeknockdoor ==
-> FirstAsk

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