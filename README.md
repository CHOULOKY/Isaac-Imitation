# The Binding of Isaac, imitation game
[![license](https://img.shields.io/badge/License-MIT-red)](https://github.com/CHOULOKY/Isaac-Imitation?tab=MIT-1-ov-file)
[![code](https://img.shields.io/badge/Code-C%23-purple)](https://dotnet.microsoft.com/ko-kr/platform/free)
[![IDE](https://img.shields.io/badge/IDE-VS-blueviolet)](https://visualstudio.microsoft.com/ko/vs/)
[![Engine](https://img.shields.io/badge/Engine-Unity(22.3.23f1)-black?logo=unity&logoColor=white)](https://unitysquare.co.kr/etc/eula)
[![Server](https://img.shields.io/badge/Server-Photon-004480?logo=Photon&logoColor=white)](https://doc.photonengine.com/ko-kr/server/current/operations/licenses)
[![Collabo](https://img.shields.io/badge/Collabo-Git-F05032?logo=Git&logoColor=white)](https://git-scm.com/)
[![Collabo](https://img.shields.io/badge/Collabo-GithubDesktop-purple?logo=github&logoColor=purple)](https://docs.github.com/ko/desktop)

<br><br><br>

## 1. Overview 
- Project Name: The Binding of Isaac, imitation game
- Description: The Binding of Isaac, imitation game using a Photon Server to add multi-play function
- Development Period : 2024.09.28 ~ 2024.00.00

<br><br>

## 2. Project Members
|Team Leader|Developer|Developer|Developer|
|:--:|:--:|:--:|:--:|
|박인희<sub>Park Inhee</sub>|김가빈<sub>Kim Garbin</sub>|황재연<sub>Hwang Jaeyeon</sub>|최태성<sub>Choi Taesung</sub>|
|![pngtree-recycle-garbage-bag-png-image_6491460](https://github.com/user-attachments/assets/ad653ad3-e628-42f2-92b1-85f7daaff750)|![다운로드](https://github.com/user-attachments/assets/f8d4e10d-f847-4170-a6fc-af61cf8fbe99)|![다운로드](https://github.com/user-attachments/assets/f8d4e10d-f847-4170-a6fc-af61cf8fbe99)|![다운로드](https://github.com/user-attachments/assets/f8d4e10d-f847-4170-a6fc-af61cf8fbe99)|
|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/CHOULOKY)|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/PeachCoffee)|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/kkiyakk)|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/11010912)|

<br><br>

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

<br><br>

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

<br><br>

## 5. Directory Structure
```

```

<br><br>

## 6. Workflow & Convention
### 6.1 Branch Strategy
#### GIT-FLOW Strategy
|[![스크린샷 2024-09-23 005247](https://github.com/user-attachments/assets/21243ecd-91e5-4c83-a068-f0038ebdd377)](https://inpa.tistory.com/entry/GIT-%E2%9A%A1%EF%B8%8F-github-flow-git-flow-%F0%9F%93%88-%EB%B8%8C%EB%9E%9C%EC%B9%98-%EC%A0%84%EB%9E%B5)|
|--|

|Branch|From|To|Description|
|:--:|:--:|:--:|--|
|master|X|X|항상 배포 가능한 상태로 유지하며, administration만이 병합할 수 있도록 한다.| 
|develop|X|X|다음 버전을 배포하기 전에 사용하는 master 브랜치이다. 역시 administration만이 병합할 수 있도록 한다.|
|release|develop|develop<br>master|배포를 위한 최종적인 버그 수정 등의 개발을 수행한다. 이후 배포 가능한 상태가 되면 master 브랜치로 병합시키고, 출시된 master 브랜치에 버전 태그(ex, v1.0, v0.2)를 추가한다.<br>* 단, 이름의 명명은 <release-{version}>로(으로) 한다. 이때, {version}은 master 브랜치에 적용될 버전이다.|
|feature|develop|develop|새로 변경될 개발코드를 분리하고, 기능을 다 완성할 때까지 유지한다. 이후 완성되면 develop 브랜치로 merge 하고 결과가 좋지 못하면 삭제한다.<br>* 단, 이름의 명명은 <feature-{feature name}>로(으로) 한다. 이때, {feature name}은 기능의 이름이다.|
|hotfix|develop|develop<br>master<br>(release)|제품에서 버그가 발생했을 경우에는 처리를 위해 이 가지로 해당 정보들을 모아준다. 버그에 대한 수정이 완료된 후에는 develop, master에 곧장 반영해주며 tag를 통해 관련 정보를 기록해둔다.<br>* 단, 이름의 명명은 <hotfix-{version}>로(으로) 한다. 이때, {version}은 master 브랜치에 적용될 버전이다.|

|Note|
|--|
|* 초기 배포 전 개발 단계에서는 develop, feature, hotfix 브랜치만 사용한다.|
|* 상업적 목적이 없는 프로젝트이므로, 최종 배포(프로젝트 마무리)에는 master 브랜치를 제한 나머지 브랜치는 삭제한다.|

<br>

### 6.2 Commit Convention
#### Commit Structure
```
type : Subject      # 제목은 커밋에 대한 주요 내용을 간결하고 명확하게 작성한다.
                    # 제목은 항상 입력해야 하며, type과 함께 작성되어야 한다.
                    # 제목은 최대 50자를 넘지 않도록 하고, 제목의 마지막에 마침표(.)를 찍지 않는다.
                    # Type은 항상 영문 소문자로 작성하며, 바로 아래의 표를 참고하여 작성한다.

body                # 커밋한 날짜(ex, 24.09.23)와 상세 내역을 작성한다.
                    # 본문의 내용은 '무엇을', '왜'에 맞춰서 작성한다.
```

#### Commit Type
|Type|Description|
|:--:|--|
|feat|새로운 기능을 추가한 경우|
|fix|버그를 수정한 경우|
|docs|문서(주석)을 추가/수정한 경우<br>- 다시 말해, 직접적인 코드 변화 없이 문서만 추가/수정했을 때)|
|style|UI를 추가/수정하거나, 스타일 관련 작업의 경우<br>- 다시 말해, 직접적인 코드 변화 없이 UI나 오브젝트 등을 추가/수정 수정했을 때|
|refactor|기능의 변경 없이, 코드를 리팩토링한 경우|
|test|테스트 코드를 추가/수정한 경우|
|chore|배포, 빌드, 프로젝트 설정과 같이 프로젝트의 기타 작업들에 대해 추가/수정한 경우|

#### Commit Example
ex1
```
feat: Add Player move

24.09.23
플레이어 캐릭터 이동 기능 개발
```
ex2
```
fix: 플레이어 벽 이동 버그

24.09.23
플레이어 캐릭터가 벽을 통과하는 버그 수정
```
ex3
```
chore: Add Player's Attack Asset

24.09.24
플레이어 캐릭터 공격 모션을 위한 에셋 추가
```

<br>

### 6.3 Folder, Asset Convention
#### Folder Name: Pascal Case, Plural Form
```
|-Scripts
|  |-SC_GameManager
|  |-SC_Player
|  |-SC_Monster
```

#### Asset Name: Pascal Case, Snake Case
```
SC_GameManager
AM_Player
TX_PlayerIdle
```

<br>

### 6.4 Code Convention
#### Variable Syntax
```
// Good
private Vector2 inputVec;

// Bad
Vector2 inputVec = default;
```
```
// Good
private Vector2 inputVec;
private void Awake() {
  inputVec = Vector2.zero;
}

// Bad
private Vector2 inputVec = default;
```

#### Variable Name: Camel Case
```
// More than two words: a + B + C
private int inputVec;
private string playerItemName;

// Bool: is + name
private bool isMove;
private bool isJump;

// Array: name + s
private GameOjbect[] players;
private List[] monsters;
```

#### Function Syntax
```
// Good
private void FixedUpdate() {
  return;
}

// Bad
void FixedUpdate() {
  return;
}
```

#### Function Name: Pascal Case
```
// More than two words: a + B + C
private void FixedUpdate() {
  return;
}
```

#### Block Syntax
```
// Good
if(true) {
  return gameOjbect;
}

// Bad
if(true) return gameObject;
```

<br><br><br><br>

## Reference
<img align="right" src="https://github.com/user-attachments/assets/141c54f0-2640-4423-b313-8dde2cfa098c" width="75" height="75" />

- <https://inpa.tistory.com/entry/GIT-%E2%9A%A1%EF%B8%8F-github-flow-git-flow-%F0%9F%93%88-%EB%B8%8C%EB%9E%9C%EC%B9%98-%EC%A0%84%EB%9E%B5>
- <https://velog.io/@hanganda23/Git-Commit-%EB%A9%94%EC%84%B8%EC%A7%80-%EC%8A%A4%ED%83%80%EC%9D%BC>
