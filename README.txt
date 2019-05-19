Extension of Arma3 : Web Player (Audio)

-------------------------------------------------------
Function Radio Play: 
_packet = format['RADIO_PLAY%1%2%3%4%5', toString [10], 'French', toString [10], 'Skyrock', toString [10]];
'RadioArma' callExtension _packet;

Param 1 = available language : French
Param 2 = available radio : Skyrock,NRJ,Nostalgie,CherieFM,RireEtChansons,FunRadio,RadioFG
-------------------------------------------------------

-------------------------------------------------------
Function Radio Stop: 
_packet = format['RADIO_STOP%1%2%3%4%5', toString [10], '', toString [10], '', toString [10]];
'RadioArma' callExtension _packet;
-------------------------------------------------------

-------------------------------------------------------
Function Youtube Play : 
_packet = format['YOUTUBE_PLAY%1%2%3%4%5', toString [10], 'zgBEVbDzuu4', toString [10], '', toString [10]];
'RadioArma' callExtension _packet;

Param 1 = zgBEVbDzuu4 is Id of Youtube Media
-------------------------------------------------------

-------------------------------------------------------
Function Youtube Stop : 
_packet = format['YOUTUBE_STOP%1%2%3%4%5', toString [10], '', toString [10], '', toString [10]];
'RadioArma' callExtension _packet;
-------------------------------------------------------

-------------------------------------------------------
Function Youtube Search (Return) : 
_packet = format['YOUTUBE_SEARCH%1%2%3%4%5', toString [10], 'soprano fragile', toString [10], '1', toString [10]];
'RadioArma' callExtension _packet;

Param 1 = text of search on youtube
Param 2 = page of number search

Return = all video (name,duration,author,id) on page called
-------------------------------------------------------

-------------------------------------------------------
Function Volume : 
_packet = format['SOUND_VOLUME%1%2%3%4%5', toString [10], '50', toString [10], '', toString [10]];
'RadioArma' callExtension _packet;

Param 1 = Pourcent Number of Sound Volume
-------------------------------------------------------



Source : https://github.com/maca134/arma-nradio


BattlEye is blocked extension but is non whitelist , a whitelist request will be made once the entire extension is finished
