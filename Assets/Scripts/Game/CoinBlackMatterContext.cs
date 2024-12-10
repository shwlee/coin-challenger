using UnityEngine;

public class CoinBlackMatterContext : CoinContext
{
    private Animator animator;

    // Start is called before the first frame update
    void Awake()
    {
        type = CoinType.BlackMatter;

        animator = GetComponent<Animator>();        
    }

    // 클릭으로 생성된 코인 초기화
    public void InitializeInRuntime()
    {
        // 클릭으로 생성된 코인은 등장 애니메이션 후 꿈틀 애니메이션
        animator.SetTrigger("BlackMatterAppear");
    }
}
