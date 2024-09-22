# The Binding of Isaac, imitation game
[![license](https://img.shields.io/badge/License-MIT-red)](https://github.com/CHOULOKY/Isaac-Imitation?tab=MIT-1-ov-file)
[![code](https://img.shields.io/badge/Code-C%23-blueviolet)](https://dotnet.microsoft.com/ko-kr/platform/free)
[![Engine](https://img.shields.io/badge/Engine-Unity(22.3.23f1)-black?logo=unity&logoColor=white)](https://unitysquare.co.kr/etc/eula)
[![Server](https://img.shields.io/badge/Server-Photon-004480?logo=Photon&logoColor=white)](https://doc.photonengine.com/ko-kr/server/current/operations/licenses)
[![Collabo](https://img.shields.io/badge/Collabo-Git-F05032?logo=Git&logoColor=white)](https://git-scm.com/)
<br>

## 1. Overview 
- Project Name: The Binding of Isaac, imitation game
- Description: The Binding of Isaac, imitation game using a Photon Server to add multi-play function
- Development Period : 2024.09.28 ~ 2024.00.00
<br>

## 2. Project Members
|Team Leader|Developer|Developer|Developer|
|:--:|:--:|:--:|:--:|
|박인희<sub>Park Inhee</sub>|김가빈<sub>Kim Garbin</sub>|황재연<sub>Hwang Jaeyeon</sub>|최태성<sub>Choi Taesung</sub>|
|![pngtree-recycle-garbage-bag-png-image_6491460](https://github.com/user-attachments/assets/ad653ad3-e628-42f2-92b1-85f7daaff750)|![다운로드](https://github.com/user-attachments/assets/f8d4e10d-f847-4170-a6fc-af61cf8fbe99)|![다운로드](https://github.com/user-attachments/assets/f8d4e10d-f847-4170-a6fc-af61cf8fbe99)|![다운로드](https://github.com/user-attachments/assets/f8d4e10d-f847-4170-a6fc-af61cf8fbe99)|
|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/CHOULOKY)|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/PeachCoffee)|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/kkiyakk)|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/11010912)|
<br>

## 3. Role
||박인희<sub>Park Inhee</sub>|김가빈<sub>Kim Garbin</sub>|황재연<sub>Hwang Jaeyeon</sub>|최태성<sub>Choi Taesung</sub>|
|--|--|--|--|--|
|Project Planning & Management|O|X|X|X|
|Team Reading & Communication|O|X|X|X|
|Player Development|O|O|X|X|
|Monster Development|O|X|X|O|
|Item Development|O|X|O|X|
|Room<sub>Map</sub> Development|O|X|X|X|
|And So On|O|O|O|O|
|Problem Solving|O|O|O|O|
<br>

## 4. Key Features
- **Random Room**
  - ﻿4방향이 있는 시작 지점에서 각 방향에 맞는 방을 랜덤으로 생성하면서 뻗어나가는 기능
- **Room Management**
  - 보스 룸이 시작 지점 옆에 배치되는 것을 막고, 각 룸이 클리어된 방인지 아닌지를 확인하는 등의 룸 관리 기능
- **Player Control**
  - 플레이어가 플레이어 캐릭터를 조작할 수 있는 기능(이동, 공격, 아이템 사용 등)
- **Monster AI**
  - 몬스터와 보스를 FSM(유한 상태 기계) 디자인 패턴과 팩토리 디자인 패턴 등을 사용하여, 플레이어를 자동으로 감지 및 추격하고 공격하는 기능
- **Item Management**
  - 각 룸이나 오브젝트 등이 보상을 드랍하는 여부와 플레이어가 획득할 아이템 및 획득한 아이템을 관리하는 기능
- **Multi-Play**
  - Photon 서버를 사용하여 두 플레이어가 같은 게임 공간에서 플레이할 수 있는 기능
- **﻿Save Game Information**
  - 사용자의 개인 저장 공간을 이용하여 플레이어의 정보(설정 정보 등)를 저장하는 기능
<br>

## 5. Directory Structure
```

```
<br>

## 6. Workflow
### Branch Strategy
- Master Branch
- hotfix : master 브랜치에서 발생한 버그를 수정하는 브랜치

<br>

## Reference

<!--

/![code](https://img.shields.io/badge/Code-C++-%2300599C.svg?logo=c%2B%2B&logoColor=white)
![code](https://img.shields.io/badge/Code-C%23-%23239120.svg?logo=csharp&logoColor=white)<br>
![Unity](https://img.shields.io/badge/Engine-unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white)/

<img align="right" src="https://github.com/user-attachments/assets/141c54f0-2640-4423-b313-8dde2cfa098c" width="75" height="75" />
<a href="https://github.com/anuraghazra/github-readme-stats">
  <img src="https://github-readme-stats.vercel.app/api?username=CHOULOKY&show_icons=true&theme=merko&count_private=true" width=50% />
  <img src="https://github-readme-stats.vercel.app/api/top-langs/?username=CHOULOKY&layout=compact&theme=merko&count_private=true" width=38% />
</a>

/<a href="https://github.com/ashutosh00710/github-readme-activity-graph">
  <img src="https://github-readme-activity-graph.vercel.app/graph?username=CHOULOKY&theme=merko&count_private=true" width=84.5%/>
</a>/

<a href="https://solved.ac/profile/chouloky">
  <img src="http://mazassumnida.wtf/api/v2/generate_badge?boj=CHOULOKY" width=49.5% />
</a>
<a href="https://solved.ac/profile/ppagnin">
  <img src="http://mazassumnida.wtf/api/v2/generate_badge?boj=PPAGNIN" width=49.5% />
</a>
-->
