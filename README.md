# HY_Traveler_Scripts

## 주요 기술
### TCP 소켓 통신
* 카메라로 촬영한 이미지와 위치 정보를 주고 받기 위해 TCP 소켓 통신을 활용.
* [LINK](https://github.com/kyj0701/HY_Traveler_Scripts/blob/main/Scripts/Socket.cs)

### 비동기 프로그래밍
* 서버가 사진을 받아 특징점을 계산하고 좌표를 계산해내는 시간이 4~5초 소요.
* 시간 동안 앱을 멈추지 않기 위해 비동기 방식으로 서버에 사진을 송신.
* [LINK](https://github.com/kyj0701/HY_Traveler_Scripts/blob/main/Scripts/Async.cs)
