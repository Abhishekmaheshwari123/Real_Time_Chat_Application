¯
`C:\chat application\src\Service\MediaService\ConnectHub.Media.API\Controllers\MediaController.cs
	namespace 	

ConnectHub
 
. 
Media 
. 
API 
. 
Controllers *
;* +
[ 
	Authorize 

]
 
[ 
ApiController 
] 
[		 
Route		 
(		 
$str		 
)		 
]		 
public

 
class

 
MediaController

 
:

 
ControllerBase

 -
{ 
private 
readonly 
IBlobStorageService (
_blobStorageService) <
;< =
public 

MediaController 
( 
IBlobStorageService .
blobStorageService/ A
)A B
{ 
_blobStorageService 
= 
blobStorageService 0
;0 1
} 
[ 
HttpPost 
( 
$str 
) 
] 
public 

async 
Task 
< 
IActionResult #
># $
Upload% +
(+ ,
	IFormFile, 5
file6 :
): ;
{ 
if 

( 
file 
== 
null 
|| 
file  
.  !
Length! '
==( *
$num+ ,
), -
return 

BadRequest 
( 
$str 1
)1 2
;2 3
using 
var 
stream 
= 
file 
.  
OpenReadStream  .
(. /
)/ 0
;0 1
var 
fileUrl 
= 
await 
_blobStorageService /
./ 0
UploadFileAsync0 ?
(? @
stream@ F
,F G
fileH L
.L M
FileNameM U
,U V
fileW [
.[ \
ContentType\ g
)g h
;h i
return 
Ok 
( 
new 
{ 
url 
= 
fileUrl %
}& '
)' (
;( )
} 
} â4
LC:\chat application\src\Service\MediaService\ConnectHub.Media.API\Program.cs
var		 
builder		 
=		 
WebApplication		 
.		 
CreateBuilder		 *
(		* +
args		+ /
)		/ 0
;		0 1
builder 
. 
Services 
. 
	AddScoped 
< 
IBlobStorageService .
,. /
BlobStorageService0 B
>B C
(C D
)D E
;E F
builder 
. 
Services 
. 
AddControllers 
(  
)  !
;! "
builder 
. 
Services 
. 
AddAuthorization !
(! "
)" #
;# $
builder 
. 
Services 
. #
AddEndpointsApiExplorer (
(( )
)) *
;* +
builder 
. 
Services 
. 
AddSwaggerGen 
( 
options &
=>' )
{ 
options 
. 

SwaggerDoc 
( 
$str 
, 
new  
OpenApiInfo! ,
{ 
Title 
= 
$str &
,& '
Version 
= 
$str 
, 
Description 
= 
$str ?
} 
) 
; 
options 
. !
AddSecurityDefinition !
(! "
$str" *
,* +
new, /!
OpenApiSecurityScheme0 E
{ 
In 

= 
ParameterLocation 
. 
Header %
,% &
Description 
= 
$str 2
,2 3
Name 
= 
$str 
, 
Type   
=   
SecuritySchemeType   !
.  ! "
Http  " &
,  & '
BearerFormat!! 
=!! 
$str!! 
,!! 
Scheme"" 
="" 
$str"" 
}## 
)## 
;## 
options$$ 
.$$ "
AddSecurityRequirement$$ "
($$" #
new$$# &&
OpenApiSecurityRequirement$$' A
{%% 
{&& 	
new'' !
OpenApiSecurityScheme'' %
{(( 
	Reference)) 
=)) 
new)) 
OpenApiReference))  0
{** 
Type++ 
=++ 
ReferenceType++ (
.++( )
SecurityScheme++) 7
,++7 8
Id,, 
=,, 
$str,, !
}-- 
}.. 
,.. 
new// 
string// 
[// 
]// 
{// 
}// 
}00 	
}11 
)11 
;11 
}22 
)22 
;22 
builder44 
.44 
Services44 
.44 
AddCors44 
(44 
options44  
=>44! #
{55 
options66 
.66 
AddDefaultPolicy66 
(66 
policy66 #
=>66$ &
policy77 
.77 
WithOrigins77 
(77 
$str88 '
,88' (
$str99 '
,99' (
$str:: '
,::' (
$str;; '
,;;' (
$str<< '
,<<' (
$str== '
,==' (
$str>> <
)>>< =
.?? 
AllowAnyHeader?? 
(?? 
)?? 
.@@ 
AllowAnyMethod@@ 
(@@ 
)@@ 
.AA 
AllowCredentialsAA 
(AA  
)AA  !
)AA! "
;AA" #
}BB 
)BB 
;BB 
builderEE 
.EE 
ServicesEE 
.EE 
AddAuthenticationEE "
(EE" #
JwtBearerDefaultsEE# 4
.EE4 5 
AuthenticationSchemeEE5 I
)EEI J
.FF 
AddJwtBearerFF 
(FF 
optionsFF 
=>FF 
{GG 
optionsHH 
.HH %
TokenValidationParametersHH )
=HH* +
newHH, /%
TokenValidationParametersHH0 I
{II 	
ValidateIssuerJJ 
=JJ 
trueJJ !
,JJ! "
ValidateAudienceKK 
=KK 
trueKK #
,KK# $
ValidateLifetimeLL 
=LL 
trueLL #
,LL# $$
ValidateIssuerSigningKeyMM $
=MM% &
trueMM' +
,MM+ ,
ValidIssuerNN 
=NN 
builderNN !
.NN! "
ConfigurationNN" /
[NN/ 0
$strNN0 <
]NN< =
,NN= >
ValidAudienceOO 
=OO 
builderOO #
.OO# $
ConfigurationOO$ 1
[OO1 2
$strOO2 @
]OO@ A
,OOA B
IssuerSigningKeyPP 
=PP 
newPP " 
SymmetricSecurityKeyPP# 7
(PP7 8
EncodingPP8 @
.PP@ A
UTF8PPA E
.PPE F
GetBytesPPF N
(PPN O
builderPPO V
.PPV W
ConfigurationPPW d
[PPd e
$strPPe n
]PPn o
)PPo p
)PPp q
}QQ 	
;QQ	 

}RR 
)RR 
;RR 
varTT 
appTT 
=TT 	
builderTT
 
.TT 
BuildTT 
(TT 
)TT 
;TT 
appVV 
.VV 

UseSwaggerVV 
(VV 
)VV 
;VV 
appWW 
.WW 
UseSwaggerUIWW 
(WW 
optionsWW 
=>WW 
{XX 
optionsYY 
.YY 
SwaggerEndpointYY 
(YY 
$strYY 6
,YY6 7
$strYY8 Q
)YYQ R
;YYR S
optionsZZ 
.ZZ 
RoutePrefixZZ 
=ZZ 
stringZZ  
.ZZ  !
EmptyZZ! &
;ZZ& '
}[[ 
)[[ 
;[[ 
app]] 
.]] 

UseRouting]] 
(]] 
)]] 
;]] 
app__ 
.__ 
UseCors__ 
(__ 
)__ 
;__ 
appaa 
.aa 
UseAuthenticationaa 
(aa 
)aa 
;aa 
appbb 
.bb 
UseAuthorizationbb 
(bb 
)bb 
;bb 
appdd 
.dd 
MapControllersdd 
(dd 
)dd 
;dd 
appff 
.ff 
Runff 
(ff 
)ff 	
;ff	 
à,
`C:\chat application\src\Service\MediaService\ConnectHub.Media.API\Services\BlobStorageService.cs
	namespace 	

ConnectHub
 
. 
Media 
. 
API 
. 
Services '
;' (
public		 
class		 
BlobStorageService		 
:		  !
IBlobStorageService		" 5
{

 
private 
readonly 
BlobServiceClient &
_blobServiceClient' 9
;9 :
private 
readonly 
string 
_containerName *
;* +
public 

BlobStorageService 
( 
IConfiguration ,
configuration- :
): ;
{ 
var 
connectionString 
= 
configuration ,
[, -
$str- L
]L M
;M N
_containerName 
= 
configuration &
[& '
$str' C
]C D
??E G
$strH T
;T U
if 

( 
string 
. 
IsNullOrWhiteSpace %
(% &
connectionString& 6
)6 7
)7 8
{ 	
throw 
new %
InvalidOperationException /
(/ 0
$str0 ]
)] ^
;^ _
} 	
_blobServiceClient 
= 
new  
BlobServiceClient! 2
(2 3
connectionString3 C
)C D
;D E
} 
public 

async 
Task 
< 
string 
> 
UploadFileAsync -
(- .
Stream. 4

fileStream5 ?
,? @
stringA G
fileNameH P
,P Q
stringR X
contentTypeY d
)d e
{ 
if 

( 

fileStream 
== 
null 
) 
throw  %
new& )!
ArgumentNullException* ?
(? @
nameof@ F
(F G

fileStreamG Q
)Q R
)R S
;S T
if 

( 
string 
. 
IsNullOrWhiteSpace %
(% &
fileName& .
). /
)/ 0
throw1 6
new7 :
ArgumentException; L
(L M
$strM i
,i j
nameofk q
(q r
fileNamer z
)z {
){ |
;| }
var 
containerClient 
= 
_blobServiceClient 0
.0 1"
GetBlobContainerClient1 G
(G H
_containerNameH V
)V W
;W X
await   
containerClient   
.   "
CreateIfNotExistsAsync   4
(  4 5
)  5 6
;  6 7
var"" 
safeFileName"" 
="" 
Path"" 
.""  
GetFileName""  +
(""+ ,
fileName"", 4
)""4 5
;""5 6
var## 

blobClient## 
=## 
containerClient## (
.##( )
GetBlobClient##) 6
(##6 7
$"##7 9
{##9 :
Guid##: >
.##> ?
NewGuid##? F
(##F G
)##G H
}##H I
$str##I J
{##J K
safeFileName##K W
}##W X
"##X Y
)##Y Z
;##Z [
var%% 
blobHttpHeader%% 
=%% 
new%%  
BlobHttpHeaders%%! 0
{%%1 2
ContentType%%3 >
=%%? @
contentType%%A L
??%%M O
$str%%P j
}%%k l
;%%l m
await&& 

blobClient&& 
.&& 
UploadAsync&& $
(&&$ %

fileStream&&% /
,&&/ 0
new&&1 4
BlobUploadOptions&&5 F
{&&G H
HttpHeaders&&I T
=&&U V
blobHttpHeader&&W e
}&&f g
)&&g h
;&&h i
if(( 

((( 
!(( 

blobClient(( 
.(( 
CanGenerateSasUri(( )
)(() *
{)) 	
return** 

blobClient** 
.** 
Uri** !
.**! "
ToString**" *
(*** +
)**+ ,
;**, -
}++ 	
var-- 

sasBuilder-- 
=-- 
new-- 
BlobSasBuilder-- +
{.. 	
BlobContainerName// 
=// 
containerClient//  /
./// 0
Name//0 4
,//4 5
BlobName00 
=00 

blobClient00 !
.00! "
Name00" &
,00& '
Resource11 
=11 
$str11 
,11 
StartsOn22 
=22 
DateTimeOffset22 %
.22% &
UtcNow22& ,
.22, -

AddMinutes22- 7
(227 8
-228 9
$num229 :
)22: ;
,22; <
	ExpiresOn33 
=33 
DateTimeOffset33 &
.33& '
UtcNow33' -
.33- .
AddDays33. 5
(335 6
$num336 7
)337 8
}44 	
;44	 


sasBuilder66 
.66 
SetPermissions66 !
(66! "
BlobSasPermissions66" 4
.664 5
Read665 9
)669 :
;66: ;
return88 

blobClient88 
.88 
GenerateSasUri88 (
(88( )

sasBuilder88) 3
)883 4
.884 5
ToString885 =
(88= >
)88> ?
;88? @
}99 
}:: 