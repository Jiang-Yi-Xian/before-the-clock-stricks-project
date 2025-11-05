EXTERNAL HasMemory(key)

== PoliceEnter ==

~ temp hasMemory = HasMemory("SavedWife")
{
    -hasMemory:
    -> LeadRoleSaveWife
}
-> LeadRoleNotSaveWife

== LeadRoleSaveWife ==
你們就是在搶劫案中殺害我父母的人
只要你們交出那時的項鍊我就放你們一馬
快交出項鍊!!
--#delay = 1
我們沒有殺人!
也不知道什麼項鍊你不要污衊我們!!
--#delay = 1
還不成承認是吧?
妳不說我就開槍了!!
--#delay = 1
你們先冷靜這之中一定有什麼誤會
--#delay = 0.5
你閉嘴!!
-> END

== LeadRoleNotSaveWife ==
我是警察
--#delay = 1
警察先生我妻子遇害了請你幫幫她
--#delay = 1
不要再演戲了!!
你們就是在搶劫案中殺害我父母的人
只要你們交出那時的項鍊我就放你們一馬
快交出項鍊!!
--#delay = 2
-> choice

== choice ==
*[詢問妻子]
-> askWife
*[直接救起妻子]
-> saveWife
*[詢問警察為何要項鍊]
-> askPolice
*[搶奪警察的手槍]
-> takeGun
-> END

== askWife ==
妳知道項鍊的位置嗎?
--#delay = 0.5
妻子(已死亡)
--#delay = 1
快點把項鍊交出來!!
--#delay = 1
都這種時候了你還在意你的項鍊!!
--#delay = 1
那對我來說很重要!!
-> END

== saveWife ==
(不理會警察先去救妻子)
--#delay = 1
不要浪費我時間!!
-> END

== askPolice ==
那條項鍊對你來說很重要嗎??
--#delay = 1
對你和我都很重要!!
快交出來不然我就開槍了!!
--#delay = 1
我馬上去找，不要開槍......
--#delay = 1
沒時間了!
-> END

== takeGun ==
拿來吧你
--#delay = 0.5
你在幹嘛!!
-> END
