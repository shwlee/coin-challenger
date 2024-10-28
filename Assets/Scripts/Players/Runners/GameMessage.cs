public class GameMessage
{
    public int turn;
    public int position;
    public int[] map;
    public int current;
} // JsonUtility 는 case 변경 지원 안 함... -ㅅ-;;


public class MoveNextResult
{
    public int direction;
}