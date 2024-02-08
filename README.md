# Chzzk-Chat-Unity
WebSocket을 이용해 유니티에서 치지직의 채팅 정보를 가져올 수 있는 C# 스크립트입니다.

## 기능
1. **현재 방송 상태 받아오기**
- 방송 제목, 방송 여부, 현재 시청자 수, 누적 시청자 수, 성인 제한 여부, 카테고리 등
2. **시청자의 채팅 정보 받아오기**
- 통신을 시작한 이후의 채팅 정보만 받아옵니다.
- 시청자의 프로필, 전송한 메시지 내용 등

단순히 채팅 정보를 받고 저장해주는 스크립트이므로 많은 기능이 존재하지 않습니다.  
개발하고자 하는 프로그램에 따라 적절히 사용하시면 됩니다.

## 사용법
1. websocket-sharp을 적용시킵니다.
2. 해당 스크립트를 오브젝트에 넣습니다.
3. **channelId** 변수에 치지직 채널 Url의 Id를 넣어줍니다.
- channelId를 제외한 변수에는 별도의 값을 넣지 않아도 됩니다.
- (예시: https://chzzk.naver.com/0390b2ceba895176cd35e59d30c8a867 중 **0390b2ceba895176cd35e59d30c8a867**)   
![image](https://github.com/server-123/Chzzk-Chat-Unity/assets/73692229/0ff2de4d-09e8-419c-94ed-ea1507560f3b)
4. 프로그램을 실행합니다.

### stopConnect
stopConnect가 true일 경우 연결이 해제된 후엔 자동으로 재연결하지 않습니다.

### 현재 방송 상태
현재 방송 상태의 정보는 status에 저장됩니다.  
![image](https://github.com/server-123/Chzzk-Chat-Unity/assets/73692229/eea61019-dfa9-4aff-ba73-145037561295)  

### 시청자의 채팅 정보
시청자의 채팅 정보는 Data의 bdy에 저장됩니다.  
profile에 닉네임, 역할, 배지 등의 정보가 저장됩니다.  
cmd의 값에 따라 93101(일반 채팅), 93102(후원 채팅), 94008(클린봇에 의해 차단된 채팅)입니다.
![image](https://github.com/server-123/Chzzk-Chat-Unity/assets/73692229/cbfec9b6-f2bf-452f-ab9a-6fa97092e430)  
동시에 많은 채팅 정보를 받을 시 bdy 배열에 한 번에 저장될 수 있습니다.

## 의존성
- https://github.com/sta/websocket-sharp
