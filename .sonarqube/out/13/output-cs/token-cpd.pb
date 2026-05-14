Þ
[C:\chat application\src\Service\ChatService\ConnectHub.Chat.Infrastructure\ChatDbContext.cs
	namespace 	

ConnectHub
 
. 
Chat 
. 
Infrastructure (
;( )
public 
class 
ChatDbContext 
: 
	DbContext &
{ 
public 

ChatDbContext 
( 
DbContextOptions )
<) *
ChatDbContext* 7
>7 8
options9 @
)@ A
:		 	
base		
 
(		 
options		 
)		 
{

 
} 
public 

DbSet 
< 
Message 
> 
Messages "
{# $
get% (
;( )
set* -
;- .
}/ 0
} ¯

bC:\chat application\src\Service\ChatService\ConnectHub.Chat.Infrastructure\ChatDbContextFactory.cs
	namespace 	

ConnectHub
 
. 
Chat 
. 
Infrastructure (
;( )
public 
class  
ChatDbContextFactory !
:" #'
IDesignTimeDbContextFactory$ ?
<? @
ChatDbContext@ M
>M N
{ 
public		 

ChatDbContext		 
CreateDbContext		 (
(		( )
string		) /
[		/ 0
]		0 1
args		2 6
)		6 7
{

 
var 
optionsBuilder 
= 
new  #
DbContextOptionsBuilder! 8
<8 9
ChatDbContext9 F
>F G
(G H
)H I
;I J
optionsBuilder 
. 
	UseNpgsql  
(  !
$str	 ¿
,
¿ À
x 
=> 
x 
. 
MigrationsAssembly %
(% &
$str& ;
); <
) 	
;	 

return 
new 
ChatDbContext  
(  !
optionsBuilder! /
./ 0
Options0 7
)7 8
;8 9
} 
} ±
TC:\chat application\src\Service\ChatService\ConnectHub.Chat.Infrastructure\Class1.cs
	namespace 	

ConnectHub
 
. 
Chat 
. 
Infrastructure (
;( )
public 
class 
Class1 
{ 
} 