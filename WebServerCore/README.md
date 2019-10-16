# WebServerCore 

앤파크 내에서 ASP.NET Core 프로젝트 서버 개발에 필요한 기본 템플릿 소스를 제공하는 것을 목표로 구현이 된 프로젝트다.

## Feature 

- ASP.NET Core 로 개발되었으며 내장된 웹서버인 Kestrel를 이용하여 윈도우 및 리눅스의 **멀티 플랫폼**을 지원가능한 서버

- 네트워크 패킷 정의에 대한 내용을 자동으로 **문서화** 하는 시스템과 각 데이터 검증도 **자동화** 하는 프로세스 적용

- 비즈니스 로직과 서버 로직의 **명확한 분리**로 구현된 전체 API에 대해서 유닛 테스트 가능한 구조

> 테스트 코드 개발로 인한 테스트 베드 확보로 운영 시 안정적으로 수정이 가능한 서버 지향

## Operating System


WebServerCore는 멀티 플랫폼 서버로 운영체제의 제약은 없으며 .NET Core 2.0 을 사용하기 위해 각 개발 환경 및 런쳐 환경에 맞게 알맞은 버전을 설치 하면 된다.

- .NET Core Runtime 2.0.* 버전
- .NET Core SDK 2.0.* 버전 or 허용되는 .NET Core SDK 2.1.* 버전

> 상세한 정보 - [.NET Core 2.0 - Supported OS Version](https://github.com/dotnet/core/blob/master/release-notes/2.0/2.0-supported-os.md)

## Dependencies

##### Nuget Libraries

| Library | Version | Description |
|---------|---------|-------------|
| CommandLineParser | 2.3.0 | Argument 처리 |
| EnyimMemcachedCore | 2.1.10 | Memcached Client 사용 |
| log4net | 2.0.8 | 로그 처리 |
| Newtonsoft.Json | 11.0.2 | Json 처리 |
| StackExchange.Redis | 3.5.0 | Redis Client 사용 |
| Swashbuckle.AspNetCore | 3.0.0 | Swagger Api Controller 페이지 |

##### Reference Projects

| Project | Description |
|---------|-------------|
| WebSharedLib | 클라이언트와 공용으로 사용하는 패킷 관련 처리 |


## File Structure

```

WebServerCore
├──Cache                    # 게임서버 캐쉬 데이터 관련 
│  ├──PBTable               # 기획 데이터 캐쉬 저장 
│  └──CacheManager.cs       # 각 캐쉬 관리하는 매니저
│
├──Common                   # 공통 설정 및 유틸 클래스 정보
│  ├──Define                # Server Define 정보
│  ├──HttpRelay             # 외부 서버 호출 처리 제공
│  └── ...                  # 각종 Util 클래스
│
├──Controllers              # Web Api Controllers
│
├──Core                     # WebServerCore 의 프로세스 관련 
│  ├──Attribute             # Custom Attribute 정보 
│  ├──KestrelServer         # Kestrel 웹 서버 내부 설정 관련 
│  ├──MiddleWare            # Controller Middleware 구현
│  ├──Session               # 세션 처리 관련 
│  ├──DBService.cs          # 컨트롤러 내 DB 담당
│  ├──IDBService.cs         # 컨트롤러 내 DB 담당 인터페이스 제공
│  ├──WebService.cs         # 컨트롤러 내 Api 처리 담당
│  └──IWebService.cs        # 컨트롤러 내 Api 처리 담당 인터페이스 제공
│
├──Database                 # 데이터 베이스 처리
│  ├──Executor              # DB 실행 부분
│  ├──External              # 외부 DB 처리
│  │  ├──Memcached          # 외부 DB Memcached 처리 부분
│  │  └──Redis              # 외부 DB Redis 처리 부분
│  ├──Utils                 # DB 내에서 쓰이는 유틸 정보
│  └── ...                  # 각각의 접근 데이터베이스 처리 부분
│
├──Logic                    # 컨트롤러 내의 공용 로직 부분
│  ├──ConsumeProcess.cs     # 재화 및 아이템 사용 부분
│  └──RewardProcess.cs      # 재화 및 아이템 보상 부분
│
├──Models                   # 데이터 오브젝트 정보
│  ├──PB                    # 기획 데이터 로드에 필요한 objects
│  └── ...                  # 처리에 필요한 objects
│
├──AppConfig.cs             # 설정 데이터 정보
├──appsettings.json         # 설정 데이터 정보
├──certificate.pfx          # SSL 인증 파일
├──DynamicConfig.cs         # 동적 설정 데이터 정보 
├──dynamicsettings.json     # 동적 설정 데이터 정보
├──IMPROVED_INFO.md         # 기존 서버 대비 개선된 문서
├──log4net.config           # 로그 설정 정보
├──program.cs               # 프로그램 진입점
├──README.md                # 서버 문서 파일
└──startup.cs               # ASP.NET Core의 설정 처리

```


## Components & Linked Services

#### 1. Core

WebServerCore를 활용한 Api 개발에 주요한 처리 소스로 Api 구현 편의와 함께 서버 내에서 이루어지는 중요 로직 및 설정 부분을 담당

##### Middleware

Api Controller의 미들웨어 구성으로 Api가 비즈니스 로직과 분리해서 패킷 Identity 부여나 로그, 네크워크 사이즈 체크등의 기능을 함

##### WebService Class

비즈니스 로직의 시작과 끝을 다루며 내부에서 패킷 헤더 및 암호화, 압축등의 Api 처리의 모든 부분을 처리하는 클래스

#### 2. Server

- Kestrel Web Server : ASP.NET Core에 내장된 웹서버로 가볍고 빠른 성능이 장점임
  
> SSL은 자체 서명된 인증서로 임시 사용 중이다.

#### 3. Database

- SQL Server : 메인 데이터베이스로 유저 정보 및 게임, 운영에 필요한 데이터 저장 
- Memcached : 유저 세션 관련 데이터 저장소
- Redis : 유저 랭킹 관련 데이터 저장소

> 각 DB 설정 관련 정보는 appsettings.json 파일에 있다.

#### 4. Logger( log4net )

기본(INFO ~ FATAL)로그, 에러 및 워닝(WARN ~ ERROR)로그, 시스템 내부 오류(WARN ~ FATAL)에 대한 로그 등 상세한 로그 정보를 파일로 제공

서버 구동 시 설정 정보를 남기는 로그 파일도 제공

> 각 로그파일은 보관정책을 가지고 있으며 기본로그는 90일 이외의 다른 로그는 30일의 보관정책을 가진다. 

#### 5. Swagger

Web Api Controller 의 웹 페이지로 서버 구동 시 디폴트로 사용이 되며 Api 호출 뿐만아니라 각 Api 별 문서 제공으로도 사용되고 있음

클라이언트 연동에 필요한 Api와 관리자 내부에 사용되는 Api등 을 구분 해서 제공

> Swagger 관련 상세 정보 - [Swagger](https://docs.microsoft.com/ko-kr/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-2.0)

#### 6. Lineworks Alarm Bot

앤파크에서 사용하는 그룹웨어인 라인 웍스의 봇과 연동해 서버의 Exception 상황이나 크리티컬한 에러에 대해서 알림을 받을 수 있음

> 알람 서버 설정 상세 정보 - [AlarmServer](http://183.110.18.41/magu-mobile-server/alarm-server/blob/master/AlarmServer/README.md)

## Issues



