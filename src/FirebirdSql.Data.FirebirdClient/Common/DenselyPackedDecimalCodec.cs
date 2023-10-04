/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Diagnostics;
using System.Numerics;

namespace FirebirdSql.Data.Common;

// based on Jaybird's implementation
class DenselyPackedDecimalCodec
{
	internal const int DigitsPerGroup = 3;
	internal const int BitsPerGroup = 10;
	internal const int NegativeSignum = -1;
	const int BitPerByte = 8;
	static readonly BigInteger OneThousand = new BigInteger(1000);

	//@formatter:off
	// Generated using org.firebirdsql.decimal.generator.GenerateLookupTable
	static readonly char[] DPDGroupBits2Digits = (
			"000" + "001" + "002" + "003" + "004" + "005" + "006" + "007" +
			"008" + "009" + "080" + "081" + "800" + "801" + "880" + "881" +
			"010" + "011" + "012" + "013" + "014" + "015" + "016" + "017" +
			"018" + "019" + "090" + "091" + "810" + "811" + "890" + "891" +
			"020" + "021" + "022" + "023" + "024" + "025" + "026" + "027" +
			"028" + "029" + "082" + "083" + "820" + "821" + "808" + "809" +
			"030" + "031" + "032" + "033" + "034" + "035" + "036" + "037" +
			"038" + "039" + "092" + "093" + "830" + "831" + "818" + "819" +
			"040" + "041" + "042" + "043" + "044" + "045" + "046" + "047" +
			"048" + "049" + "084" + "085" + "840" + "841" + "088" + "089" +
			"050" + "051" + "052" + "053" + "054" + "055" + "056" + "057" +
			"058" + "059" + "094" + "095" + "850" + "851" + "098" + "099" +
			"060" + "061" + "062" + "063" + "064" + "065" + "066" + "067" +
			"068" + "069" + "086" + "087" + "860" + "861" + "888" + "889" +
			"070" + "071" + "072" + "073" + "074" + "075" + "076" + "077" +
			"078" + "079" + "096" + "097" + "870" + "871" + "898" + "899" +
			"100" + "101" + "102" + "103" + "104" + "105" + "106" + "107" +
			"108" + "109" + "180" + "181" + "900" + "901" + "980" + "981" +
			"110" + "111" + "112" + "113" + "114" + "115" + "116" + "117" +
			"118" + "119" + "190" + "191" + "910" + "911" + "990" + "991" +
			"120" + "121" + "122" + "123" + "124" + "125" + "126" + "127" +
			"128" + "129" + "182" + "183" + "920" + "921" + "908" + "909" +
			"130" + "131" + "132" + "133" + "134" + "135" + "136" + "137" +
			"138" + "139" + "192" + "193" + "930" + "931" + "918" + "919" +
			"140" + "141" + "142" + "143" + "144" + "145" + "146" + "147" +
			"148" + "149" + "184" + "185" + "940" + "941" + "188" + "189" +
			"150" + "151" + "152" + "153" + "154" + "155" + "156" + "157" +
			"158" + "159" + "194" + "195" + "950" + "951" + "198" + "199" +
			"160" + "161" + "162" + "163" + "164" + "165" + "166" + "167" +
			"168" + "169" + "186" + "187" + "960" + "961" + "988" + "989" +
			"170" + "171" + "172" + "173" + "174" + "175" + "176" + "177" +
			"178" + "179" + "196" + "197" + "970" + "971" + "998" + "999" +
			"200" + "201" + "202" + "203" + "204" + "205" + "206" + "207" +
			"208" + "209" + "280" + "281" + "802" + "803" + "882" + "883" +
			"210" + "211" + "212" + "213" + "214" + "215" + "216" + "217" +
			"218" + "219" + "290" + "291" + "812" + "813" + "892" + "893" +
			"220" + "221" + "222" + "223" + "224" + "225" + "226" + "227" +
			"228" + "229" + "282" + "283" + "822" + "823" + "828" + "829" +
			"230" + "231" + "232" + "233" + "234" + "235" + "236" + "237" +
			"238" + "239" + "292" + "293" + "832" + "833" + "838" + "839" +
			"240" + "241" + "242" + "243" + "244" + "245" + "246" + "247" +
			"248" + "249" + "284" + "285" + "842" + "843" + "288" + "289" +
			"250" + "251" + "252" + "253" + "254" + "255" + "256" + "257" +
			"258" + "259" + "294" + "295" + "852" + "853" + "298" + "299" +
			"260" + "261" + "262" + "263" + "264" + "265" + "266" + "267" +
			"268" + "269" + "286" + "287" + "862" + "863" + "888" + "889" +
			"270" + "271" + "272" + "273" + "274" + "275" + "276" + "277" +
			"278" + "279" + "296" + "297" + "872" + "873" + "898" + "899" +
			"300" + "301" + "302" + "303" + "304" + "305" + "306" + "307" +
			"308" + "309" + "380" + "381" + "902" + "903" + "982" + "983" +
			"310" + "311" + "312" + "313" + "314" + "315" + "316" + "317" +
			"318" + "319" + "390" + "391" + "912" + "913" + "992" + "993" +
			"320" + "321" + "322" + "323" + "324" + "325" + "326" + "327" +
			"328" + "329" + "382" + "383" + "922" + "923" + "928" + "929" +
			"330" + "331" + "332" + "333" + "334" + "335" + "336" + "337" +
			"338" + "339" + "392" + "393" + "932" + "933" + "938" + "939" +
			"340" + "341" + "342" + "343" + "344" + "345" + "346" + "347" +
			"348" + "349" + "384" + "385" + "942" + "943" + "388" + "389" +
			"350" + "351" + "352" + "353" + "354" + "355" + "356" + "357" +
			"358" + "359" + "394" + "395" + "952" + "953" + "398" + "399" +
			"360" + "361" + "362" + "363" + "364" + "365" + "366" + "367" +
			"368" + "369" + "386" + "387" + "962" + "963" + "988" + "989" +
			"370" + "371" + "372" + "373" + "374" + "375" + "376" + "377" +
			"378" + "379" + "396" + "397" + "972" + "973" + "998" + "999" +
			"400" + "401" + "402" + "403" + "404" + "405" + "406" + "407" +
			"408" + "409" + "480" + "481" + "804" + "805" + "884" + "885" +
			"410" + "411" + "412" + "413" + "414" + "415" + "416" + "417" +
			"418" + "419" + "490" + "491" + "814" + "815" + "894" + "895" +
			"420" + "421" + "422" + "423" + "424" + "425" + "426" + "427" +
			"428" + "429" + "482" + "483" + "824" + "825" + "848" + "849" +
			"430" + "431" + "432" + "433" + "434" + "435" + "436" + "437" +
			"438" + "439" + "492" + "493" + "834" + "835" + "858" + "859" +
			"440" + "441" + "442" + "443" + "444" + "445" + "446" + "447" +
			"448" + "449" + "484" + "485" + "844" + "845" + "488" + "489" +
			"450" + "451" + "452" + "453" + "454" + "455" + "456" + "457" +
			"458" + "459" + "494" + "495" + "854" + "855" + "498" + "499" +
			"460" + "461" + "462" + "463" + "464" + "465" + "466" + "467" +
			"468" + "469" + "486" + "487" + "864" + "865" + "888" + "889" +
			"470" + "471" + "472" + "473" + "474" + "475" + "476" + "477" +
			"478" + "479" + "496" + "497" + "874" + "875" + "898" + "899" +
			"500" + "501" + "502" + "503" + "504" + "505" + "506" + "507" +
			"508" + "509" + "580" + "581" + "904" + "905" + "984" + "985" +
			"510" + "511" + "512" + "513" + "514" + "515" + "516" + "517" +
			"518" + "519" + "590" + "591" + "914" + "915" + "994" + "995" +
			"520" + "521" + "522" + "523" + "524" + "525" + "526" + "527" +
			"528" + "529" + "582" + "583" + "924" + "925" + "948" + "949" +
			"530" + "531" + "532" + "533" + "534" + "535" + "536" + "537" +
			"538" + "539" + "592" + "593" + "934" + "935" + "958" + "959" +
			"540" + "541" + "542" + "543" + "544" + "545" + "546" + "547" +
			"548" + "549" + "584" + "585" + "944" + "945" + "588" + "589" +
			"550" + "551" + "552" + "553" + "554" + "555" + "556" + "557" +
			"558" + "559" + "594" + "595" + "954" + "955" + "598" + "599" +
			"560" + "561" + "562" + "563" + "564" + "565" + "566" + "567" +
			"568" + "569" + "586" + "587" + "964" + "965" + "988" + "989" +
			"570" + "571" + "572" + "573" + "574" + "575" + "576" + "577" +
			"578" + "579" + "596" + "597" + "974" + "975" + "998" + "999" +
			"600" + "601" + "602" + "603" + "604" + "605" + "606" + "607" +
			"608" + "609" + "680" + "681" + "806" + "807" + "886" + "887" +
			"610" + "611" + "612" + "613" + "614" + "615" + "616" + "617" +
			"618" + "619" + "690" + "691" + "816" + "817" + "896" + "897" +
			"620" + "621" + "622" + "623" + "624" + "625" + "626" + "627" +
			"628" + "629" + "682" + "683" + "826" + "827" + "868" + "869" +
			"630" + "631" + "632" + "633" + "634" + "635" + "636" + "637" +
			"638" + "639" + "692" + "693" + "836" + "837" + "878" + "879" +
			"640" + "641" + "642" + "643" + "644" + "645" + "646" + "647" +
			"648" + "649" + "684" + "685" + "846" + "847" + "688" + "689" +
			"650" + "651" + "652" + "653" + "654" + "655" + "656" + "657" +
			"658" + "659" + "694" + "695" + "856" + "857" + "698" + "699" +
			"660" + "661" + "662" + "663" + "664" + "665" + "666" + "667" +
			"668" + "669" + "686" + "687" + "866" + "867" + "888" + "889" +
			"670" + "671" + "672" + "673" + "674" + "675" + "676" + "677" +
			"678" + "679" + "696" + "697" + "876" + "877" + "898" + "899" +
			"700" + "701" + "702" + "703" + "704" + "705" + "706" + "707" +
			"708" + "709" + "780" + "781" + "906" + "907" + "986" + "987" +
			"710" + "711" + "712" + "713" + "714" + "715" + "716" + "717" +
			"718" + "719" + "790" + "791" + "916" + "917" + "996" + "997" +
			"720" + "721" + "722" + "723" + "724" + "725" + "726" + "727" +
			"728" + "729" + "782" + "783" + "926" + "927" + "968" + "969" +
			"730" + "731" + "732" + "733" + "734" + "735" + "736" + "737" +
			"738" + "739" + "792" + "793" + "936" + "937" + "978" + "979" +
			"740" + "741" + "742" + "743" + "744" + "745" + "746" + "747" +
			"748" + "749" + "784" + "785" + "946" + "947" + "788" + "789" +
			"750" + "751" + "752" + "753" + "754" + "755" + "756" + "757" +
			"758" + "759" + "794" + "795" + "956" + "957" + "798" + "799" +
			"760" + "761" + "762" + "763" + "764" + "765" + "766" + "767" +
			"768" + "769" + "786" + "787" + "966" + "967" + "988" + "989" +
			"770" + "771" + "772" + "773" + "774" + "775" + "776" + "777" +
			"778" + "779" + "796" + "797" + "976" + "977" + "998" + "999"
	).ToCharArray();

	// from ICU decNumber decDPD.h
	static readonly int[] Bin2DPD = {
			0,      1,    2,    3,    4,    5,    6,    7,
			8,      9,   16,   17,   18,   19,   20,   21,   22,   23,   24,   25,   32,
			33,    34,   35,   36,   37,   38,   39,   40,   41,   48,   49,   50,   51,
			52,    53,   54,   55,   56,   57,   64,   65,   66,   67,   68,   69,   70,
			71,    72,   73,   80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
			96,    97,   98,   99,  100,  101,  102,  103,  104,  105,  112,  113,  114,
			115,  116,  117,  118,  119,  120,  121,   10,   11,   42,   43,   74,   75,
			106,  107,   78,   79,   26,   27,   58,   59,   90,   91,  122,  123,   94,
			95,   128,  129,  130,  131,  132,  133,  134,  135,  136,  137,  144,  145,
			146,  147,  148,  149,  150,  151,  152,  153,  160,  161,  162,  163,  164,
			165,  166,  167,  168,  169,  176,  177,  178,  179,  180,  181,  182,  183,
			184,  185,  192,  193,  194,  195,  196,  197,  198,  199,  200,  201,  208,
			209,  210,  211,  212,  213,  214,  215,  216,  217,  224,  225,  226,  227,
			228,  229,  230,  231,  232,  233,  240,  241,  242,  243,  244,  245,  246,
			247,  248,  249,  138,  139,  170,  171,  202,  203,  234,  235,  206,  207,
			154,  155,  186,  187,  218,  219,  250,  251,  222,  223,  256,  257,  258,
			259,  260,  261,  262,  263,  264,  265,  272,  273,  274,  275,  276,  277,
			278,  279,  280,  281,  288,  289,  290,  291,  292,  293,  294,  295,  296,
			297,  304,  305,  306,  307,  308,  309,  310,  311,  312,  313,  320,  321,
			322,  323,  324,  325,  326,  327,  328,  329,  336,  337,  338,  339,  340,
			341,  342,  343,  344,  345,  352,  353,  354,  355,  356,  357,  358,  359,
			360,  361,  368,  369,  370,  371,  372,  373,  374,  375,  376,  377,  266,
			267,  298,  299,  330,  331,  362,  363,  334,  335,  282,  283,  314,  315,
			346,  347,  378,  379,  350,  351,  384,  385,  386,  387,  388,  389,  390,
			391,  392,  393,  400,  401,  402,  403,  404,  405,  406,  407,  408,  409,
			416,  417,  418,  419,  420,  421,  422,  423,  424,  425,  432,  433,  434,
			435,  436,  437,  438,  439,  440,  441,  448,  449,  450,  451,  452,  453,
			454,  455,  456,  457,  464,  465,  466,  467,  468,  469,  470,  471,  472,
			473,  480,  481,  482,  483,  484,  485,  486,  487,  488,  489,  496,  497,
			498,  499,  500,  501,  502,  503,  504,  505,  394,  395,  426,  427,  458,
			459,  490,  491,  462,  463,  410,  411,  442,  443,  474,  475,  506,  507,
			478,  479,  512,  513,  514,  515,  516,  517,  518,  519,  520,  521,  528,
			529,  530,  531,  532,  533,  534,  535,  536,  537,  544,  545,  546,  547,
			548,  549,  550,  551,  552,  553,  560,  561,  562,  563,  564,  565,  566,
			567,  568,  569,  576,  577,  578,  579,  580,  581,  582,  583,  584,  585,
			592,  593,  594,  595,  596,  597,  598,  599,  600,  601,  608,  609,  610,
			611,  612,  613,  614,  615,  616,  617,  624,  625,  626,  627,  628,  629,
			630,  631,  632,  633,  522,  523,  554,  555,  586,  587,  618,  619,  590,
			591,  538,  539,  570,  571,  602,  603,  634,  635,  606,  607,  640,  641,
			642,  643,  644,  645,  646,  647,  648,  649,  656,  657,  658,  659,  660,
			661,  662,  663,  664,  665,  672,  673,  674,  675,  676,  677,  678,  679,
			680,  681,  688,  689,  690,  691,  692,  693,  694,  695,  696,  697,  704,
			705,  706,  707,  708,  709,  710,  711,  712,  713,  720,  721,  722,  723,
			724,  725,  726,  727,  728,  729,  736,  737,  738,  739,  740,  741,  742,
			743,  744,  745,  752,  753,  754,  755,  756,  757,  758,  759,  760,  761,
			650,  651,  682,  683,  714,  715,  746,  747,  718,  719,  666,  667,  698,
			699,  730,  731,  762,  763,  734,  735,  768,  769,  770,  771,  772,  773,
			774,  775,  776,  777,  784,  785,  786,  787,  788,  789,  790,  791,  792,
			793,  800,  801,  802,  803,  804,  805,  806,  807,  808,  809,  816,  817,
			818,  819,  820,  821,  822,  823,  824,  825,  832,  833,  834,  835,  836,
			837,  838,  839,  840,  841,  848,  849,  850,  851,  852,  853,  854,  855,
			856,  857,  864,  865,  866,  867,  868,  869,  870,  871,  872,  873,  880,
			881,  882,  883,  884,  885,  886,  887,  888,  889,  778,  779,  810,  811,
			842,  843,  874,  875,  846,  847,  794,  795,  826,  827,  858,  859,  890,
			891,  862,  863,  896,  897,  898,  899,  900,  901,  902,  903,  904,  905,
			912,  913,  914,  915,  916,  917,  918,  919,  920,  921,  928,  929,  930,
			931,  932,  933,  934,  935,  936,  937,  944,  945,  946,  947,  948,  949,
			950,  951,  952,  953,  960,  961,  962,  963,  964,  965,  966,  967,  968,
			969,  976,  977,  978,  979,  980,  981,  982,  983,  984,  985,  992,  993,
			994,  995,  996,  997,  998,  999, 1000, 1001, 1008, 1009, 1010, 1011, 1012,
		   1013, 1014, 1015, 1016, 1017,  906,  907,  938,  939,  970,  971, 1002, 1003,
			974,  975,  922,  923,  954,  955,  986,  987, 1018, 1019,  990,  991,   12,
			13,   268,  269,  524,  525,  780,  781,   46,   47,   28,   29,  284,  285,
			540,  541,  796,  797,   62,   63,   44,   45,  300,  301,  556,  557,  812,
			813,  302,  303,   60,   61,  316,  317,  572,  573,  828,  829,  318,  319,
			76,    77,  332,  333,  588,  589,  844,  845,  558,  559,   92,   93,  348,
			349,  604,  605,  860,  861,  574,  575,  108,  109,  364,  365,  620,  621,
			876,  877,  814,  815,  124,  125,  380,  381,  636,  637,  892,  893,  830,
			831,   14,   15,  270,  271,  526,  527,  782,  783,  110,  111,   30,   31,
			286,  287,  542,  543,  798,  799,  126,  127,  140,  141,  396,  397,  652,
			653,  908,  909,  174,  175,  156,  157,  412,  413,  668,  669,  924,  925,
			190,  191,  172,  173,  428,  429,  684,  685,  940,  941,  430,  431,  188,
			189,  444,  445,  700,  701,  956,  957,  446,  447,  204,  205,  460,  461,
			716,  717,  972,  973,  686,  687,  220,  221,  476,  477,  732,  733,  988,
			989,  702,  703,  236,  237,  492,  493,  748,  749, 1004, 1005,  942,  943,
			252,  253,  508,  509,  764,  765, 1020, 1021,  958,  959,  142,  143,  398,
			399,  654,  655,  910,  911,  238,  239,  158,  159,  414,  415,  670,  671,
			926,  927,  254,  255
		};
	//@formatter:on

	readonly int _numberOfDigits;
	readonly int _digitGroups;

	// Creates a densely packed decimal coder for the specified number of digits.
	// Current implementation only supports decoding and encoding `n * 3 + 1` number of digits with
	// `n > 0`, where the most significant digit is provided by the caller during decoding.
	public DenselyPackedDecimalCodec(int numberOfDigits)
	{
		if (numberOfDigits / DigitsPerGroup <= 0 || numberOfDigits % DigitsPerGroup != 1)
			throw new ArgumentOutOfRangeException(nameof(numberOfDigits), $"{nameof(numberOfDigits)} must be of form n * 3 + 1 with n > 0, was {numberOfDigits}.");
		_numberOfDigits = numberOfDigits;
		_digitGroups = numberOfDigits / DigitsPerGroup;
	}

	// Decodes a densely packed decimal from a byte array to a BigInteger.
	// Digits are read from the end of the array to the front.
	public BigInteger DecodeValue(int signum, int firstDigit, byte[] decBytes)
	{
		return DecodeValue(signum, firstDigit, decBytes, decBytes.Length - 1);
	}

	// Decodes a densely packed decimal from a byte array to a BigInteger.
	// Digits are read from `lsbIndex` of the array to the front.
	public BigInteger DecodeValue(int signum, int firstDigit, byte[] decBytes, int lsbIndex)
	{
		if (firstDigit < 0 || firstDigit > 9)
			throw new ArgumentOutOfRangeException(nameof(firstDigit), $"{nameof(firstDigit)} must be in range 0 <= firstDigit <= 9, was {firstDigit}.");
		ValidateLsbIndex(lsbIndex, decBytes.Length);
		return DecodeValue0(signum, firstDigit, decBytes, lsbIndex);
	}

	// Encodes a BigInteger to a densely packed decimal in a byte array.
	// Digits are written from the end of the array to the front. The most significant digit is not encoded
	// into the array, but instead returned to the caller.
	public int EncodeValue(BigInteger value, byte[] decBytes)
	{
		return EncodeValue(BigInteger.Abs(value), decBytes, decBytes.Length - 1);
	}

	// Encodes a BigInteger to a densely packed decimal in a byte array.
	// Digits are written from `lsbIndex` of the array to the front. The most significant digit is not encoded
	// into the array, but instead returned to the caller.
	public int EncodeValue(BigInteger value, byte[] decBytes, int lsbIndex)
	{
		ValidateLsbIndex(lsbIndex, decBytes.Length);

		return EncodeValue0(BigInteger.Abs(value), decBytes, lsbIndex);
	}

	BigInteger DecodeValue0(int signum, int firstDigit, byte[] decBytes, int lsbIndex)
	{
		var digitChars = CreateZeroedCharArray();
		for (var digitGroup = 0; digitGroup < _digitGroups; digitGroup++)
		{
			// Each digit group is 10 bits in two bytes in the array as [.., second, first, ..],
			// moving to the left for next digit groups. If there are unconsumed bits in the second byte,
			// the second byte becomes the first byte of the next group.
			var digitBitsFromEnd = digitGroup * BitsPerGroup;
			var firstByteBitOffset = digitBitsFromEnd % BitPerByte;
			var firstByteIndex = lsbIndex - digitBitsFromEnd / BitPerByte;

			var dpdGroupBits = 0x3FF & (
					(decBytes[firstByteIndex] & 0xFF) >>> firstByteBitOffset
							| decBytes[firstByteIndex - 1] << BitPerByte - firstByteBitOffset);

			if (dpdGroupBits != 0)
			{
				Array.Copy(DPDGroupBits2Digits, dpdGroupBits * DigitsPerGroup, digitChars, digitChars.Length - (digitGroup + 1) * DigitsPerGroup, DigitsPerGroup);
			}
		}
		if (firstDigit != 0)
		{
			digitChars[1] = char.Parse(firstDigit.ToString());
		}

		return ToBigInteger(signum, digitChars);
	}

	int EncodeValue0(BigInteger value, byte[] decBytes, int lsbIndex)
	{
		var remainingValue = value;
		for (var digitGroup = 0; digitGroup < _digitGroups; digitGroup++)
		{
			// Each digit group is 10 bits in two bytes in the array as [.., second, first, ..],
			// moving to the left for next digit groups. If there are unconsumed bits in the second byte,
			// the second byte becomes the first byte of the next group.
			var digitBitsFromEnd = digitGroup * BitsPerGroup;
			var firstByteBitOffset = digitBitsFromEnd % BitPerByte;
			var firstByteIndex = lsbIndex - digitBitsFromEnd / BitPerByte;

			remainingValue = BigInteger.DivRem(remainingValue, OneThousand, out var remainder);
			var currentGroup = Bin2DPD[(int)remainder];

			decBytes[firstByteIndex] =
					(byte)(decBytes[firstByteIndex] | (currentGroup << firstByteBitOffset));
			decBytes[firstByteIndex - 1] =
					(byte)(decBytes[firstByteIndex - 1] | (currentGroup >>> BitPerByte - firstByteBitOffset));
		}
		var mostSignificantDigit = (int)remainingValue;
		Debug.Assert(0 <= mostSignificantDigit && mostSignificantDigit <= 9, $"{nameof(mostSignificantDigit)} out of range, was {mostSignificantDigit}.");
		return mostSignificantDigit;
	}

	char[] CreateZeroedCharArray()
	{
		var digitChars = new char[_numberOfDigits + 1];
		for (var i = 0; i < digitChars.Length; i++)
		{
			digitChars[i] = '0';
		}
		return digitChars;
	}

	void ValidateLsbIndex(int lsbIndex, int decBytesLength)
	{
		if (lsbIndex < 0 || lsbIndex >= decBytesLength)
		{
			throw new IndexOutOfRangeException($"{nameof(lsbIndex)} must be within array {nameof(decBytesLength)} with length of {decBytesLength}, was {lsbIndex}.");
		}
		if ((lsbIndex + 1) * BitPerByte < BitsPerGroup * _digitGroups)
		{
			throw new ArgumentException($"Need at least {(BitsPerGroup * _digitGroups + 7) / BitPerByte} bytes for value, have {lsbIndex + 1} (lsbIndex = {lsbIndex})");
		}
	}

	static BigInteger ToBigInteger(int signum, char[] digitChars)
	{
		var digitCharIndex = FindFirstNonZero(digitChars);
		if (digitCharIndex == -1)
		{
			// All zeroes
			return BigInteger.Zero;
		}
		if (signum == NegativeSignum)
		{
			digitChars[--digitCharIndex] = '-';
		}
		var s = new string(digitChars, digitCharIndex, digitChars.Length - digitCharIndex);
		return BigInteger.Parse(s);
	}

	static int FindFirstNonZero(char[] digitChars)
	{
		for (var index = 0; index < digitChars.Length; index++)
		{
			if (digitChars[index] != '0')
			{
				return index;
			}
		}
		return -1;
	}
}
