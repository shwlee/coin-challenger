// sample
class JsRunner {
    constructor() {
        this._column = 0;
        this._row = 0;
        this._myNumber;
    }

    // Player 를 초기화 합니다. 게임 환경을 인자로 전달받습니다. 전달받은 인자는 게임 동안 유지해야합니다.
    // int: myNumber = 할당받은 플레이어 번호. (플레이 순서)
    // int: column = 현재 생성된 보드의 열.
    // int: row = 현재 생성된 보드의 행.
    // 메서드명은 반드시 아래 메서드와 일치해야합니다.
    initialize(myNumber, column, row) {
        this._myNumber = myNumber;
        this._column = column;
        this._row = row;
    }

    // Player 의 이름을 반환합니다. 현재 플레이어의 이름을 반환합니다. (하드 코딩하는 것을 추천)
    // 메서드명은 반드시 아래 메서드와 일치해야합니다.
    getName() {
        return "javascript player";
    }

    // 현재 턴에서 진행할 방향을 결정합니다. map 정보와 현재 플레이어의 위치를 전달받은 후 다음 이동 방향을 반환해야 합니다.
    // 결정 방향은 left, up, right, down 순서로 0, 1, 2, 3 정수로 표현해야합니다.// 
    // map: int[]  1차원 배열로 표현된 현재 map 정보.
    // myPosition:int 현재 플레이어의 위치. map 배열의 인덱스로 표시됨
    // returns:이번 프레임에 진행할 방향. left, up, right, down 순서오 0, 1, 2, 3 으로 표현.
    // 메서드명은 반드시 아래 메서드와 일치해야합니다.
    moveNext(map, myPosition) {
        let min = 0;
        let max = 3;
        min = Math.ceil(min);
        max = Math.floor(max);
        let selectedIndex = Math.floor(Math.random() * (max - min + 1)) + min;
        return selectedIndex;
    }

    /*
            int[] map
                1. 현재 map의 전체 정보가 1차원 배열로 전달됩니다.
                    - int 값으로 구성됩니다.
                    - map 배열의 구성은 void Initialize() 메소드에서 전달받은 row/column 형태의 grid 를 1차원 배열로 전환한 형태입니다.
            예시> void Initialize() 에 column = 6, row = 6 가 전달되었다면 int[] map 은 length == 24 인 1차원 배열로 전달됩니다.
        	
                2. 배열 요소
                    - 각 배열에 할당된 정수의 의미는 다음과 같습니다.
                    - 0 : 이동 가능한 공간
                    - -1 : 벽. 이동 불가능.
                    - value > 0 : 획득 가능한 코인
                        - 각 코인의 점수만큼 부여됩니다.
                    - 각 코인의 종류별 점수
                        - cooper : 10
                        - silver : 30
                        - gold : 100
                        - diamond : 200
                        - black matter : 500
                	
    
            예시> 다음은 map 정보에 따른 예시입니다.
    
        column = 6;
        row = 6; 일 때
    
        map 구성 예시.
            0	0	0	-1	0	0
            10	10	10	10	0	0
            0	0	0	-1	0	0
            0	30	30	30	30	0
            0	100	100	100	100	0
            0	0	0	-1	0	0
    
        위 map 은 int[36] 배열이 전달됩니다.
        [0,0,0,-1,0,0,10,10,10,10,0,0,0,0,0,-1,0,0,0,30,30,30,30,0,0,100,100,100,100,0,0,0,0,-1,0,0 ]
        	
        매 MoveNext() 호출마다 현재 map 의 상황이 위와 같은 형식의 배열로 전달됩니다.
    
                	
            */
}

module.exports = JsRunner; // 필수 작성.