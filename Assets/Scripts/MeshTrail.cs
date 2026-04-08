using System.Collections;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2.0f;                                 //잔상 효과 지속
    public MovementInput moveScript;                                //캐릭터의 움직임을 제어 하는 스크립트
    public float speedBoost = 6;                                    //잔상 효과 사용시에 에니메이션 속도 증가량
    public Animator animator;                                       //캐릭터의 에니메이션을 제어하는 컴포넌트
    public float animSpeedBoost = 1.5f;                             //잔상 효과 사용시 애니메이션 속도 증가량

    [Header("Mesh Releted")]
    public float meshRefreshRate = 1.0f;                            //잔상이 시작되는 시간 간격
    public float meshDestroyDelay = 3.0f;                           //생성된 잔상이 사라지는데 걸리는 시간
    public Transform positionToSpawn;                               //잔상이 생성될 위치

    [Header("Shader Releted")]
    public Material mat;                                            //잔상에 적용될 재질
    public string ShaderVerRef;                                     //셰이더에서 사용항 변수 이름(Alpha)
    public float shaderVarRate = 0.1f;                              //셰이더 효과의 변화 속도
    public float shaderVarRefreshRate = 0.05f;                      //셰이더 효과가 업데잍트 되는 시간 간격

    private SkinnedMeshRenderer[] skinnedRenderer;           //캐릭터의 3D 모델을 랜더링하는 컴포넌트들
    private bool isTrailActive;                                     //현재 잔상 효과가 활성화 되어 있는지를 확인하는 변수

    private float normalSpeed;                                      //원래 이동 속도를 저장하는 변수
    private float normalAnimSpeed;                                  //원래 애니메이션 속도를 저장하는 변수

    //재질의 투명도를 서서히 변경하는 코루틴
    IEnumerator AnimateMaterialFloat(Material m , float valueGoal, float rate, float refreshRate)
    {
        float valueToAnimate = m.GetFloat(ShaderVerRef);            //알파 값을 가져온다.

        //목표 값에 도달 할 때 까지 반복
        while(valueToAnimate > valueGoal)
        {
            valueToAnimate -= rate;
            m.SetFloat(ShaderVerRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    //잔상 효과 발동

    IEnumerator ActivateTrail(float timeActivated)
    {
        //이전 내용 변수들 저장
        normalSpeed = moveScript.movementSpeed;
        moveScript.movementSpeed = speedBoost;

        normalAnimSpeed = animator.GetFloat("animSpeed");
        animator.SetFloat("animSpeed", animSpeedBoost);

        //잔상을 남가는 로직
        while(timeActivated > 0)
        {
            if (skinnedRenderer == null)
                skinnedRenderer = positionToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();

            for(int i = 0; i < skinnedRenderer.Length; i++)
            {
                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf =gObj.AddComponent<MeshFilter>();

                Mesh m = new Mesh();
                skinnedRenderer[i].BakeMesh(m);
                mf.mesh = m;
                mr.material = mat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(gObj, meshDestroyDelay);

                
            }


            yield return new WaitForSeconds(meshRefreshRate);

        }

        moveScript.movementSpeed = normalSpeed;
        animator.SetFloat("animSpeed", normalAnimSpeed);
        isTrailActive = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }
}
