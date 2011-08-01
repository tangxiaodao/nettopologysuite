using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Simplify;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class BufferTest : BaseSamples
    {
        private const string GeomText =
            @"LINESTRING(-8.50100090092787 91.4810937292432,-10.8234006958666 91.8368026666699,-7.00247853623872 91.1844278958622,-8.68057207000981 91.6129332911536,-7.0553372455014 89.3186299239348,-8.05646550938141 90.7610182527882,-8.08614309388729 88.0873623264818,-5.775243156694 88.1494616301439,-7.82040861554539 89.4158158614959,-6.358788485372 87.597779801373,-8.02895424933043 88.5841699246614,-8.38837065932449 87.4197569074487,-6.58534420858234 88.4400374173056,-7.87808384622477 86.2538265068704,1.10908139060148 71.4281388352546,9.17321329483186 42.6779616565811,9.72126510300806 23.873012342489,11.4065985074839 20.3917587580072,10.3252053352133 19.7629814645278,10.9910404128998 17.146236994873,10.3252079789799 18.0976841371722,10.6405438058614 16.8018712749458,11.2705332739626 18.4082487187453,11.8642926840769 14.2526940457852,12.2716768173257 16.187853921372,10.8933122373225 15.9448318346965,10.6295765421074 13.7661827630694,11.028595069874 15.8547924491811,12.1748261949976 15.3789962252603,9.89764579519715 15.5938412877754,9.57262060756666 12.553879104614,29.8000354274023 0.581692693138809,28.6690759365964 2.59347055394269,28.1254150274879 0.703320693253614,30.1870626236511 1.49046965293255,30.1624556770115 0.287544621473272,29.3851810357061 1.84462967320889,30.1796292409253 -0.0617803278264307,31.9585412334784 0.470195044784962,30.7177418299129 2.29932941132564,31.0431228127132 0.0154968650050731,31.7916771327149 2.07778150133984,29.800027364704 2.53246717869723,30.0878569655041 0.14915694413559,32.5176685188077 0.257156658168479,31.5093248007087 2.7668099919884,30.0483402196978 2.55654040730731,31.2160190644115 2.41083844409881,30.7079957795585 0.398451225310302,33.1371762260525 0.581707258072782,32.2945379459635 2.86553737638544,32.5448337341779 0.581704560048749,30.5780501241835 2.62387659778654,31.7916817669592 1.03639538391079,29.4663129664669 2.63238364363101,27.1364517279596 0.748171627750854,32.1360374503194 -0.195436002149831,31.4602569330154 1.96627315490736,31.9978857284643 -0.278648541612216,33.7781428843963 -0.0002036003148681,32.4233613102305 1.92736754502891,28.1860579327802 1.14433346851672,31.1430271945131 -0.868315914956656,26.8436506066015 0.971922561451923,27.8436348433644 -1.17198791104198,29.2077005809789 -1.30564664401362,28.5402632067477 0.97818461745204,29.8834717689938 -1.30564387485068,31.2870424357698 -1.00775264629093,30.0767844756278 0.782965966461945,31.7188996899162 -1.30563603403946,31.4602612443271 0.978196767112804,32.7831558732602 -1.05863801102011,31.8774043127225 0.867178781213602,32.2945571261008 -1.41665329672577,30.8034753471774 -0.364658799504836,35.2979869053827 -1.87182047271195,34.1564432752527 -0.105174410705916,34.9437862572842 -2.70409135108977,34.3427165824113 -0.820751488238967,36.1322795661878 -3.30397199472567,39.6446209903266 -3.09301588321002,36.3430961090214 -1.85948899842589,39.8031456788377 -4.98035191386731,40.7292029642203 -4.98034674215991,39.5756686332815 -3.64014742008338,41.1463470902739 -5.19128202735259,41.1463360210936 -3.25160954203281,41.9806361902291 -5.74637630575387,41.4717086346677 -3.46254533246239,41.1463502580718 -5.74638111506174,40.0001140533288 -4.72659240660287,42.6335892327157 -5.34674140888907,41.5743811038643 -6.25950970260242,40.5903826661973 -4.02390497819942,40.9670440131915 -6.39844214878903,40.7291981531995 -4.12866847273463,39.5829754732425 -5.49263145102628,40.3037136651456 -4.35071049896428,40.3037264314532 -6.63454441462299,39.5756780853332 -5.3165466694875,41.062928050514 -6.86768174679429,40.0001203249097 -5.82568860225799,40.6457727076442 -4.68376803216931,41.1750635487235 -7.1507287024966,42.2202813023601 -5.13845316184096,40.4504309189368 -7.13234624189246,44.1305643615617 -7.81377195259966,42.7579599897089 -6.00028086778545,45.1523479920435 -8.03897894782517,44.6420086758526 -5.80504455474532,45.614459928991 -7.92784070393264,45.2260080672079 -5.68291911643501,46.2011489698144 -7.65708528396818,44.9635760235089 -5.86093195020763,44.0797890252148 -7.49096207455686,46.2332148791175 -8.0671515486681,44.1715605000955 -7.49096151292149,45.9017984467497 -8.6328676581204,45.6431552458858 -6.34903539362979,44.4969334970236 -7.71299914752646,45.2260150309768 -6.79311729092342,39.7955947374836 -6.60037693194029,46.6361891451015 -9.40565632692882,47.4762965688176 -7.35881681858855,45.5940900113125 -7.22489833256878,46.1437464364986 -9.41000483304311,44.9902111934205 -8.09201035069731,47.5777110461827 -9.73194190770132,47.5620203735954 -7.45922116471384,47.0453430373033 -9.68289279495485,48.2378089786371 -9.97619219388823,46.5608785231474 -7.69236931524583,48.9803253266675 -10.1871248388956,48.8293554741578 -7.93255921888695,47.5427676096842 -9.66563365756015,45.3177969638221 -8.4584139722421,40.6480963997528 -9.80129452162151,44.4835302876494 -11.6304116020882,42.961006746839 -9.59378744474763,30.4655364306096 -9.98869726422072,33.6542777652443 -11.9591843932352,52.0121159656599 -11.8633289182429,49.1398425946983 -9.13562089683056,38.9500581998776 -9.1330817613644,36.5712307203463 -10.72168074303,36.8915238142271 -11.9635138940328,62.5328291710916 -11.9244272284175,61.5780483162311 -9.91265304027729,52.7264898278075 -10.8052270747297,53.5689223073979 -13.0847094197405,51.5937862548401 -11.0203392580306,48.3963303500834 -11.0118497550321,46.5765779529959 -11.3560342458826,46.8946315963116 -13.6287530177609,45.9018157124329 -11.3449255160205,44.2405768927719 -11.5780885920289,45.4090332207442 -13.3354552506195,44.0580437280922 -11.5669766525657,44.2249150571216 -13.8508095197088,43.2237571224323 -11.566981702245,43.557485561821 -13.8508135826203,44.6420584915518 -13.8508069490152,44.4835298962676 -11.5669740400585,45.4763467619333 -14.0728413686349,52.7366106932062 -13.5528670194225,51.0584932914191 -11.8182373678693,52.9139509902079 -13.8366920960842,52.3258239886305 -11.5669213916473,57.9833092642713 -13.0735622139597,64.0099447156699 -11.3941661666087,64.9768541650505 -10.1552796455995,62.996323580598 -8.58040298472101,65.545576967953 -10.1480834023898,64.3385095696604 -8.03640510415447,68.3482192161432 -9.72882909714432,68.6540233995306 -7.46045145934405,69.0988596055891 -8.99361194268195,72.3294348513487 -7.60260656273608,70.7634981521603 -5.47177605324409,71.9231783359736 -7.7555984887082,74.711669473889 -6.93340961754794,72.0065707277171 -4.12842395079101,73.9524600754774 -6.04525890519001,77.1671766248986 -4.62203575742195,77.3499937878749 -2.2711037138215,75.1733607081996 -1.79975710215684,76.3524134974551 -3.82949009254467,75.1966341367378 -1.36633761443844,72.7210148143154 -1.35917686829296,74.0922766157712 -3.3036818394398,77.0685345465845 -2.96207951145722,77.9952895568956 -2.56441867132889,76.6950063550767 -0.691106579088414,77.0122705781859 -2.75965413395943,74.0922475566511 -0.475850801840284,77.1475385652886 -1.39533265636855,79.3348071816309 0.602684381138528,73.8419392190084 1.74454300281028,63.9973802772912 1.20045178970365,61.6452758216795 -0.536133628817283,61.5541765078376 1.07690843698452,59.5203018393347 -0.377239794778987,56.1634490637043 0.412145799240383,21.1993591102629 -6.60045559022155,-0.929271195133165 -13.8871330287602,3.22177713116164 -12.1107997546965,0.923523775641083 -10.7945214640634,1.50942713706865 -12.9627864626706,-0.706098538570552 -11.2729616533225,1.56455862053863 -12.6086749188777,3.69326634834875 -10.9123457149951,2.68576992502797 -8.79161534178429,1.46035569205636 -9.22339905110988,3.15198470458335 -10.9767251464514,2.10176952576937 -8.79161553567219,4.22574664571266 -9.73427820274467,1.65287439055183 -8.63207265573282,3.88464077574047 -9.66958074724918,7.65343783364434 -6.54730397477918,6.51514016880634 -4.58396181833789,8.23763066237512 -5.82507246253754,7.09913945009195 -3.79572056144997,8.6559876853154 -5.39238178887025,7.02264244922929 -3.51255752222527,8.97378179206011 -4.44054281423462,8.07157557862735 -2.40247376128452,9.20724397062992 -3.85125385074312,10.2734379603126 -2.72998608878179,8.01494801417525 -1.35783258019483,9.99267351695047 -2.77577125507702,11.3065170987342 -0.45372632840551,10.3069703717145 0.511411912378955,11.5268968637708 0.137073976370186,9.52085471091141 1.28947698389121,11.5747655715644 0.0589824798810846,8.77604400389387 4.18660624252982,7.51705682846359 3.94639990486102,9.10746673664289 2.69052634647047,6.68198908661438 4.30872579656594,7.69925415447892 2.52913780494015,6.67465606360263 4.84160984894701,7.355467822421 3.25672542400572,5.85405658266378 5.14480036902302,3.68085690402801 4.39802657118751,5.03581092548311 3.58706987158234,4.39506202753021 6.43039652148872,4.88411421356109 4.58988857953593,2.78914575948696 6.05288628676185,4.82009836806869 4.9956107423152,3.34073861524683 6.94803827113375,3.52004963342273 4.67826329202836,3.10290628708266 6.9620970570532,1.28630419804332 6.95358482819483,1.00885260863163 4.67826250334843,-0.113755531131281 6.70114687286932,-2.5358716158667 5.54223707191853,-1.05636857707756 3.63505569416862,-1.49400132359277 5.86300032803169,-0.771850116343051 4.28419505520371,0.0766676555328439 5.18891332516093,-2.17289293344743 6.28253611805081,-0.814120825167738 4.25934407642546,-1.60344369271805 6.43039536023688,-7.51490043079569 3.74572563251657,-1.69056122968304 6.23107327387847,-10.7473620859645 1.46407159811378,-8.23559121562357 0.641801478736982,-16.1633554406712 -2.96376755971342,-24.5131046095797 -10.1929756099302,-14.0974501913235 -4.38023072598848,-6.71912647990782 2.5426013103601,-11.0618153332133 2.33718918268323,-6.24311647201303 3.02011692632683,-8.62368443366485 4.06665373933399,-5.31559021946069 3.98349528447929,-2.34945272489178 6.38710326040215,2.53295452801443 10.8033568451849,0.579466122135696 12.4410394128396,2.31859815492569 10.642281262021,1.68462132586273 12.8572490425386,-4.87751842694633 9.1435845609095,-4.33056830268414 6.89866012414603,-4.58085219333069 9.1824942438066,-6.30638797492836 6.0008374924337,-4.51919434562105 5.26508172801022,-13.7496497860734 1.74417799025215,-12.7632244245295 -0.24177248487229,-14.3910679613649 -0.574941039500368,-14.4016514815198 -2.75865075779658,-14.3336552689531 -1.24225399252414,-17.2239536040689 -2.78395631095747,-17.5739499738191 -4.72668194465541,-16.1479259182935 -6.15811853821591,-16.6067242123845 -4.73975286734825,-17.3987536808881 -6.38087765198672,-16.2717481586518 -5.05889173566405,-15.3061103663167 -9.70428581759302,-15.1679537588207 -7.45936208255024,-17.1557693164185 -8.6361274096833,-15.6728555172503 -7.82249246722791,-24.5518547965256 -1.26320666893419,-23.4293621390385 -2.92998372647374,-25.1302909311002 -0.241852151500255,-27.7144014179139 -0.160467056147914,-27.1065693635848 -2.42695501294966,-27.2797876981342 0.0141755512461785,-31.9106070285877 0.116854937045172,-31.776271761846 -1.73910014775041,-30.0498003985298 -2.40596311741692,-31.4210689909093 -0.810622763481218,-32.1739440294174 -2.93921250836701,-30.4089139794233 -4.78517275919315,-30.7232081013479 -3.04116418377367,-30.075653701174 -5.76983368477427,-29.1922896727614 -4.01770459508231,-29.666405363158 -6.21733149875728,-29.1922901222561 -4.12872441318246,-30.2095556927091 -4.64406713901976,-29.1923016164638 -6.96765741468242,-29.4425781134445 -4.68382248412582,-30.5888140680684 -6.04777430595668,-28.2807146756288 -6.2349705327033,-29.3508159712389 -6.96765677115079,-30.1475592298694 -5.0395233348794,-28.5838368547186 -6.67435257526154,-29.6094352748599 -4.68382180083541,-30.1851020447013 -6.96765332670381,-29.9775072476094 -4.68262635263967,-30.9094938126958 -6.77242536458867,-30.4437210840601 -4.68381832646223,-31.6868069394184 -4.68381297056867,-31.269673939035 -6.96764870460272,-31.062078799284 -4.68262176176373,-32.3602924064409 -6.51294971745719,-31.2716466837373 -7.11384269660431,-29.7146384911119 -5.8257383272798,-30.86087376445 -6.96765046593668,-31.3550815921031 -4.2918184130735,-32.4537921457061 -6.3412767431737,-31.5658254723635 -4.26237854038619,-32.6879473934356 -4.12870941265044,-32.6879577474299 -6.41254332830572,-32.7797188255203 -4.12870899601115,-31.7725348067757 -5.84683342655829,-32.7797188255203 -4.12870899601115,-34.0928125914222 -5.49265950120783,-32.0837939404606 -4.64118981740919,-34.1980030123621 -3.79564295515134,-33.939385194043 -6.07947809471574,-34.9649831989234 -4.31098619282679,-33.1073912694488 -5.62478778455789,-34.6151458661089 -3.79564096455991,-34.4399567762473 -6.07947572106572,-35.0239458608489 -3.79563899036572,-34.4399557686842 -5.86853806648906,-33.9741813909821 -3.65780945483819,-35.0239442429822 -3.46257953606493,-34.8654410100465 -5.74641422276911,-35.4410860406071 -3.25163984311187,-35.2825829444404 -5.53547453991803,-36.356509514551 -3.91726711928831,-34.3671603430894 -5.08078478383045,-35.6649189637645 -5.67330721819994,-35.8401106260095 -3.9908598735905,-35.6974348833223 -6.1798745160444,-33.7953385406052 -4.86139482696226,-36.4545886068926 -3.82570391102896,-34.4700612818019 -4.39356151185839,-35.6792227924449 -2.96336662726097,-34.8872013950675 -3.83846041595733,-35.8582358039947 -4.6473131589037,-34.9633299080768 -3.1263863267636,-35.7670126695508 -1.31344717006631,-37.6162696500361 -1.6968642968049,-35.5546214283566 -2.1620579034335,-36.5340004984736 -3.30396997038617,-36.7072158283749 -0.88504314572768,-37.2389795942288 -3.17030223597335,-36.2025668330741 -0.697693166104467,-37.5980627476049 0.256217854915115,-36.8137870388403 -1.38419046827597,-38.8148282217748 -1.04756663905089,-37.7770695546583 -0.14307307167894,-39.5156503429038 -1.61804017390037,-37.0479896228639 -1.61805327579825,-39.535125027331 -1.47481439247691,-38.6197164057658 -3.6370185546459,-39.5156561914408 -2.71713637328112,-37.6208578597144 -1.80788409003853,-39.6898131732812 -3.34840149419737,-37.4734810924852 -2.71714727634277,-38.7323549951217 -3.94743309319184,-37.8953140470085 -1.77045300356319,-38.1942318817638 -3.85906045639077,-37.0479936810633 -2.38409002180687,-39.3907217244857 -2.19871760404553,-37.3521655186362 -3.34371795159988,-39.26432185679 -2.30791130599508,-38.4150082694213 -4.15967756749414,-36.7069844508482 -2.6409842332863,-38.8188306833498 -3.72326889543008,-36.7069856130981 -2.86302387041769,-38.5362797848203 -2.13032380276678,-38.1191491017647 -4.41415994425218,-36.0357234625761 -3.40437738811423,-37.3599331836063 -1.35319126547725,-37.4892681291974 -3.72540002852388,-36.2025725496757 -1.80789135083044,-37.3302403109338 -3.53857433971464,-33.7460572620747 -4.78515791245931,-32.1103203981489 -4.67140302327793,-33.5466913908192 -2.98985709746887,-31.5558259043199 -3.45090986435385,-33.8918975930553 -3.32906883221384,-31.6334856055959 -3.50541597101209,-32.4641181736046 -2.32557859582064,-33.1050915772079 -4.41418470951524,-31.9588549286408 -3.05023328398763,-34.2669540637116 -2.86303587979178,-33.1968630129262 -4.41418428756653,-32.1896594088623 -1.80791038319289,-32.5210880272459 -3.63704864150514,-33.6033580797402 -2.02994357028304,-31.1481906484155 -3.05414771775742,-32.1795844032939 -1.10252641900616,-38.1312860973408 -2.41935696836962,-37.7770931542814 -4.6473033605143,-39.2334154864253 -2.68395230354765,-40.3338686010347 -4.72586815799597,-34.9502942313537 -8.58303272198369,-35.6974387882279 -7.02582530632054,-34.6637941373529 -9.54790372200135,-32.8976537930616 -8.38333191224449,-35.0645711160808 -9.2631770197218,-32.0366789942139 -13.2245072527352,-18.7637537537236 -17.7477162683916,-19.9099820275604 -16.9388557061686,-19.6847012109681 -18.5938563297961,-17.9585637166922 -17.7955371743211,-20.0657110417227 -19.4498453287058,-17.7760416726296 -19.1592575900764,-20.0757984589144 -18.9609627723559,-18.9222779311867 -21.4002677729885,-18.3466135766082 -19.1164353991,-18.3466208003917 -21.9553683354295,-18.7637593669873 -19.9046750134361,-18.9222799998305 -22.1885084580385,-20.1378251149338 -21.4230521464117,-17.9294852057689 -25.2970657977118,-18.8847468580579 -23.3578312843549,-19.0951919755819 -25.2753459179623,-17.0335384124743 -24.1551510479935,-17.9294852057689 -25.2970657977118,-17.447030610248 -23.0863362278631,-25.8088147399746 -20.7802429465033,-23.8327663023038 -21.7870360371138,-24.8049877658318 -19.6703212204305,-25.2712076573947 -21.4002483157031,-24.1249721117615 -19.9252758676291,-25.5965705876091 -18.7833538594442,-25.4380640306024 -21.0671882848884,-24.2499043209764 -20.1106354414926,-26.0790197262737 -18.7676393737354,-25.2712057507223 -20.8562512235836,-24.0971534304436 -20.0573996481077,-26.1866195337174 -19.0271082617048,-25.4380640306024 -21.0671882848884,-24.6811512589542 -19.0271135722378,-26.2723426461698 -18.5724137806825,-25.8552071271825 -20.8562491530478,-26.9314297045464 -18.5724113489877,-26.8744744237986 -20.5388987387647,-28.3315958112309 -18.7676308743614,-27.3569249514561 -20.856243611609,-26.1912152768505 -20.057392345454,-26.9314284190363 -18.2282499214421,-25.7862492427381 -19.5684598222548,-28.0025771849037 -19.3701627750319,-25.7740707017158 -19.8464562015461,-27.8950379783008 -18.3619103837175,-26.2100885162484 -19.9245462001242,-25.4929867113579 -18.4453339257271,-24.2010826721551 -13.7541207494189,-25.2711862948716 -15.3052604535672,-25.4370253264601 -12.9104172447325,-24.4503226236055 -13.8302868936479,-25.4755694663156 -12.588888913406,-25.8551851913611 -14.7390573211471,-23.0712999301185 -12.8549299841164,-22.4069869821986 -10.9979933870054,-23.1020289685143 -11.8636534284739,-23.9363080266121 -9.5798168100424,-22.6624939912646 -11.1785320516996,-23.8690052024713 -11.337202038833,-23.5191640787911 -9.34667656777745,-22.5361483394932 -11.1022292616401,-21.976851871024 -9.94044967358993,-23.4009323215733 -10.8801868733935,-20.9268309945739 -10.287665747398,-22.1635043029216 -7.37348502717844,-21.3958971579736 -9.27645661629434,-20.0697554524286 -8.79913607886436,-19.2665947281836 -8.06727403619303,-21.3282438727491 -7.49106527162708,-18.4138906246517 -6.89642535482905,-18.5372117459207 -4.35219959650724,-23.8999019222647 -3.25794287796122,-22.8456715515455 -4.73666906057476,-23.6636093755467 -2.69338893522427,-35.0868598170441 1.14436374768837,-36.1979803981949 -0.850920758039618,-34.8613935544814 1.04805823398971,-43.6327238318725 2.85448401744907,-43.070108058751 0.953729084616583,-40.9927316597892 2.06391516212712,-42.5784682864638 3.07629182881222,-40.3851057948849 2.83385937175116,-40.8722575553544 1.69194514545858,-36.4381830741727 2.83219157576229,-37.9294381530966 2.96861670481046,-37.4539635407452 5.18636362906414,-38.88608562365 3.41739710866349,-36.9728715876605 4.49913835501424,-37.9522481181593 3.91232556563348,-36.8060088529966 5.59823371466044,-37.6480289310073 6.82983567036645,-37.952243443123 4.80048411881465,-35.3169906683805 10.0431706717872,-36.7577595099374 9.10343616209477,-34.9632650403672 9.85183053128839,-36.6150725141258 9.3629047252096,-33.5221725257023 8.90819549320903,-34.4774280676216 10.8363301951012,-33.5221735580365 8.68615585328476,-29.808422515101 8.37042836607974,-30.0265143768879 10.6369149570951,-29.8513241843076 8.35308025995549,-28.4252595714838 8.86842142331467,-30.1076523881796 10.1822211057924,-29.940802216757 8.57463319628101,-29.1922312833312 10.4037699075776,-28.9419552329554 8.11993493078253,-24.5327035620507 7.47896053489831,-24.5953283301077 9.74873582200942,-25.1203713682233 7.52506933032579,-23.5941955989054 7.23175688892638,-21.5168273674488 7.23175039027012,-21.8421913771126 9.51558533266189,-21.4824713623021 6.99741473714895,-20.2110602687661 9.07126469567257,-22.4044262208617 8.3736700743686,-19.9745089017141 9.04393346071111,-22.1624814776859 9.16191004891472,-20.4239125310251 8.01998793200077,-20.8410480996865 10.3038230944107,-20.1328977047077 8.21862886200636,-19.6244308819356 9.3143068301846,-20.2163170694378 11.4263154297746,-27.0006864914282 14.2815532024966,-36.3552353409297 15.2969949120073,-35.6162040231481 13.3489983531311,-37.5350429618058 15.9659015371585,-35.6932624600697 13.4248443726335,-25.5964596687115 12.4607972538927,-25.688222752781 14.7446315638841,-24.5771989695027 12.4228769836644,-20.8957689494817 11.378391786249,-7.69371476753923 11.5603014480126,-6.95440120559832 7.93158684940853,-5.32565799038489 8.47465515765611,-7.14536621287526 8.60678042815072,-5.2482823502481 5.34438326080104,-6.04264804545833 7.50731977133895,-7.08508164038792 6.4959170286165,-5.82393919657485 4.80038658554977,-7.02043657604823 6.11656264791763,-8.68066578295598 3.35217095401428,-9.9579813603676 -2.01260318273833,-9.03961949948516 -1.28117244904239,-10.4747752240724 -1.82980089209222,-9.75343265395314 -4.41425411291686,-11.2153123228926 -3.42754119964041,-10.1705766397094 -5.19139225954113,-11.8032663718949 -5.09205563877898,-9.88274132562591 -6.27894652460248,-10.8380037361298 -4.12877536411377,-11.984236521505 -5.72587176048799,-10.6711506413509 -6.8677907798761,-10.9965187216468 -4.58395637626063,-12.6748085555309 -6.27135444644544,-11.4970955943355 -7.96688570115603,-3.26047721760035 -5.13592906415615,-3.95416108413064 -5.76989532769302,1.12933096854551 -6.9288065480983,0.352062438567037 -5.36061640352824,1.42599731826764 -7.18975600641826,0.451318784538862 -5.33428873135171,0.626519003489678 -7.32759067771133,1.0088539962671 -5.23898161736523,-0.246588617787472 -6.04619241877863,1.26748309628705 -7.9668947562088,1.09228265014159 -5.68306087481584,2.18290335410954 -7.30126270877449,0.121252511268737 -6.82497791296045,1.50942609119315 -7.96689470961801,4.32422815508428 -6.38089734766364,2.92771150398547 -5.23898109354062,3.10291252071702 -7.52281493219628,2.01229158295249 -5.92681719971643,4.39724420873819 -8.07166704098978,4.14160590599568 -5.93884569464431,2.84657836342637 -7.8452595815205,2.80624582551063 -6.38808919287399,4.56981998021986 -7.893790324217,0.0535887035882392 -6.92692326149304,0.841997270218014 -9.0770929901406,-0.584066703991111 -7.41962583995676,1.91681218826658 -7.74034162824234,-2.1182858491592 -7.64360253911673,-0.771852177340898 -9.60438418704515,-0.147572754139329 -7.60131187837833,-2.27921914808765 -6.66994304573254,-1.91114751869024 -8.85505315167921,-8.02487409157545 -6.2397205363766,-6.57967892414939 -8.6302291159786,-6.86500894251262 -6.2591370086448,-15.2513786137326 -5.68304482720039,-15.2513834446449 -7.96687873726483,-18.0499127490482 -5.5110105873571,-41.2488910726608 -1.37051835027521,-38.3175737154124 -3.07204282713992,-34.6224723694712 -2.51572761889071,-23.761856200539 0.832977161115074,-23.5429012432886 3.08614284673795,-22.1828597873779 1.95269741943965,-15.168711128225 4.48550600852032,-15.3055716293016 6.82844850311133,-17.7625450593182 8.1944382682472,-19.2009916730664 5.90564713937126,-17.4311287346059 6.28538363419726,-19.4927816174429 5.38722846296172,-18.5884975753158 3.91224964182938,-19.0056345472405 5.97404504229994,-18.9222120840646 3.69021087091013,-19.9210562441948 5.40833350847044,-19.9923093528086 4.08984482977104,-17.9086172411891 3.70426717520964,-16.9416934306415 5.05416255837863,-18.1796922580552 6.19608255144655,-17.1497219446865 4.17907312706393,-18.7677080966548 5.6109129480281,-17.2619852518159 3.69020670496745,-17.7625508028952 5.86302205277126,-18.677977602761 3.25674586496215,-17.0139900261701 4.85322696642126,-18.9466735429609 4.7925790464152,-16.8289416967268 2.83131438582682,-17.9294096527441 5.08588372975396,-19.0756434627083 3.71082807645977,-16.8680769407112 2.58988583136957,-16.8448408943491 4.51968003568726,-17.9983697798922 3.57605463643149,-16.1502523076737 1.91129671884046,-16.0856425195615 4.18661884486704,-17.194686041803 0.308757822014479,-17.5039312322793 2.29928522406495,-17.1990565025757 -0.139597712305544,-17.2619890868751 2.08834698246376,-18.8659539330772 2.29077698241765,-18.9222226021424 -0.317604589311378,-19.9521928624236 1.68830309268453,-27.0436262045524 -1.07606727927436,-36.4814871787814 -12.4288898116635,-32.7861312649977 -13.2199258910664,-12.4942166061127 -3.27397969135504,-13.5077129199009 -1.35327540434783,-12.81979789612 -3.47020137539051,-10.4757869395313 -1.80350084622945,-11.690404143752 0.116793799429454,-11.8391430879146 -1.97181499162329,-11.1740049082686 0.190383152331876,-12.0894281790366 -1.63875512200727,-10.9431953170671 -0.163780532772785,-11.8651518715523 1.43451307995047,-13.1717035714742 -0.418022298819492,-11.110051430434 0.39131881569777,-11.7395878027947 1.45086715533581,-12.9345916437491 1.93535005687348,-12.5524448335703 0.149105092473822,-14.0994849042752 1.79503282265361,-11.9020772434051 0.304470873211495,-11.9520744602864 2.61099414900657,-11.2551392550535 0.581639894977082,-12.5065630939773 2.8654758987943,-13.5911335102337 2.86547786164261,-12.8327202634591 0.943969245088294,-13.5911328824149 3.19853731873532,-16.5697431039401 3.41623784806482,-6.24108472424784 1.13673290654268,-7.75847069401284 3.6314949022882,-7.65936921916189 1.35877390990138,-16.7695454771539 2.86113921484533,-13.0940378021705 0.806464290573918,-13.9288633790584 2.26920541638883,-12.0418738304381 2.23525276342764,-15.2839189689886 0.907756520281489,-13.8675132484773 20.1532448292913,-15.0093840816672 19.1108547370333)";

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferTest"/> class.
        /// </summary>
        public BufferTest() : base(GeometryFactory.Fixed) { }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestWithDefaultFactory()
        {
            PerformTest(GeometryFactory.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestWithFixedFactory()
        {
            PerformTest(GeometryFactory.Fixed);
        }

        /// <summary>
        /// 
        /// </summary>
        private void PerformTest(IGeometryFactory factory) 
        {
            IGeometry path = new WKTReader(factory).Read(GeomText);
            Assert.IsNotNull(path); 
            Debug.WriteLine(String.Format("Original Points: {0}", path.NumPoints));

            IGeometry simplified = DouglasPeuckerSimplifier.Simplify(path, 2);
            Assert.IsNotNull(simplified); 
            Debug.WriteLine(String.Format("Simplified Points: {0}", simplified.NumPoints));

            IGeometry buffered = simplified.Buffer(1.143);            
            Assert.IsNotNull(buffered);
            Debug.WriteLine(buffered);
        }

        public void BufferZero()
        {
            const string wkt = 
@"LINESTRING (0.131499700898395 -106.048825207893, 0.214925873208014
-101.719050959423, 0.214925750753543 -97.6113163891882,
0.214925621679813 -93.2815420840726, 0.131499285788604
-83.2897551168332, 0.131499245289997 -81.0693579923862,
0.214925135170241 -76.9616232870209, 0.381776611037356
-68.4130942180089, 0.632053838837485 -64.1943395560651,
0.798904549239423 -55.8678500279518, 0.965755424490026
-51.6490952821928, 1.04918049754522 -47.5413603763288, 1.216031054491
-43.4336254307751, 1.21603034291624 -39.2148706216502,
1.46630613455377 -35.3291753291882, 1.46630527652542 -31.11042046793,
1.46630444107613 -27.0026854424598, 1.63315428396527
-22.8949503541264, 1.46630272501622 -18.5651755775079,
1.46630191214483 -14.5684603384853, 1.46630107669307
-10.4607252068867, 1.38287509348789 -6.35299006384426,
1.21602410725127 -2.24525490925484, 1.04917334027541 1.64044053479631,
0.965747720953842 5.85919563202954, 0.798897253949768
9.96693087540342, 0.632046967345163 14.1856860226623,
0.632046607221801 18.2934213404512
, 0.38177179615792 22.5121765415973, 0.548621118425862
26.5088920492596, 0.214921921523676 30.8386671810771,
0.214921795756638 35.0574224826751, 0.131497046191467 39.498217565559,
-0.0353521669617239 43.6059530460827, -0.4524747423803
47.9357283251215, -0.619323417988547 52.1544837536291,
-1.11986960213068 56.2622193766346, -1.28671779462348
59.9258754694222, -1.45356574932243 63.9225912326279,
-1.70383783249951 67.808287160798, -1.87068534577926 71.9160228671952,
-2.28780528663775 75.9127387944009, -2.37122831392773
79.4653751147445, -2.53807525425783 83.3510711417379,
-2.78834607948959 87.2367672248572, -2.95519284268212
90.5673638758933, -9.1285807394459 82.6849571608342)";

            IGeometryFactory factory = GeometryFactory.Default;
            WKTReader reader = new WKTReader(factory);
            IGeometry geom = reader.Read(wkt);
            Assert.IsNotNull(geom);
            Assert.IsTrue(geom.IsValid);
            IGeometry result = geom.Buffer(0);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(GeometryCollection.Empty.Equals(result));
        }
    }
}
