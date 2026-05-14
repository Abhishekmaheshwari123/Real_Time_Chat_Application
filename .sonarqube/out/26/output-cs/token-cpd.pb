Þ
[C:\chat application\src\Service\AuthService\ConnectHub.Auth.Infrastructure\AuthDbContext.cs
	namespace 	

ConnectHub
 
. 
Auth 
. 
Infrastructure (
;( )
public 

class 
AuthDbContext 
:  
	DbContext! *
{ 
public 
AuthDbContext 
( 
DbContextOptions -
<- .
AuthDbContext. ;
>; <
options= D
)D E
:		 
base		 
(		 
options		 
)		 
{

 	
} 	
public 
DbSet 
< 
User 
> 
Users  
{! "
get# &
;& '
set( +
;+ ,
}- .
} ¯

bC:\chat application\src\Service\AuthService\ConnectHub.Auth.Infrastructure\AuthDbContextFactory.cs
	namespace 	

ConnectHub
 
. 
Auth 
. 
Infrastructure (
;( )
public 
class  
AuthDbContextFactory !
:" #'
IDesignTimeDbContextFactory$ ?
<? @
AuthDbContext@ M
>M N
{ 
public 

AuthDbContext 
CreateDbContext (
(( )
string) /
[/ 0
]0 1
args2 6
)6 7
{		 
var

 
optionsBuilder

 
=

 
new

  #
DbContextOptionsBuilder

! 8
<

8 9
AuthDbContext

9 F
>

F G
(

G H
)

H I
;

I J
optionsBuilder 
. 
	UseNpgsql  
(  !
$str	 ¾
,
¾ ¿
x 
=> 
x 
. 
MigrationsAssembly %
(% &
$str& ;
); <
)< =
;= >
return 
new 
AuthDbContext  
(  !
optionsBuilder! /
./ 0
Options0 7
)7 8
;8 9
} 
} ±
TC:\chat application\src\Service\AuthService\ConnectHub.Auth.Infrastructure\Class1.cs
	namespace 	

ConnectHub
 
. 
Auth 
. 
Infrastructure (
;( )
public 
class 
Class1 
{ 
} 