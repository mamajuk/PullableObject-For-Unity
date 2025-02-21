# PullableObject

## Overview
```PullableObject```는 고정된 줄이 당겨지는 움직임이 구현된 컴포넌트입니다. 간단하게 당겨지는 위치를 제공하는 것으로 줄을 움직일 수 있으며, 당겨진 줄은 최대길이만큼 당겨지면 팽팽해지고, 한계를 넘어서면 끊어지게 됩니다. 다음은 ```PullableObject```를 적용한 예시를 보여줍니다.

<table><tr><td>
<img width="700px" img src="https://github.com/mamajuk/PullableObject-For-Unity/blob/main/readmy_resources/readmy_gif.gif?raw=true">
</td></tr></table>

## Tutorial

<table><tr>
<td><img width="400px" height="400px" img src="https://github.com/mamajuk/PullableObject-For-Unity/blob/main/readmy_resources/readmy_image(1).png?raw=true"></td>
<td><img width="800px" height="400px" img src="https://github.com/mamajuk/PullableObject-For-Unity/blob/main/readmy_resources/readmy_image(3).png?raw=true"></td>
</tr></table>

가장 먼저 해야할 일은 줄을 구성하는 모든 ```Transform```을 ```PullableObject``` 의 ```Bone Data``` 프로퍼티에 입력하는 것입니다. 단일 연결 구조(Single Chain)라면 최상위 ```Transform``` 만 추가한 후, ```Add all child bones under the RootBone``` 버튼을 눌러 자동으로 하위 본들을 등록할 수 있습니다. 만약 메시의 본들의 ```Transform``` 을 사용할 경우 ```캔디랩( Candy wrap )``` 현상이 발생할 수 있으므로, ```MeshRenderer``` 대신 ```LineRenderer```를 사용하는 방법을 고려해볼 수 있습니다. 

줄 구성이 끝났다면, ```Holding Point``` 프로퍼티에 줄이 당겨질 위치의 ```GameObject``` 를 설정하면 줄이 해당 위치로 당겨지게 됩니다. 또한 ```C# Scripting``` 이나 ```Unity Editor Inspector``` 에서 각 줄의 상태가 변화할 때 호출되는 대리자를 활용해 유연하게 확장할 수도 있습니다.

## Scripting Example
``` c#

Transform[] tr = new Transform[10];

//...생략...

void Start()
{
   /**********************************************************
   *   줄을 구성하는 연속적인 Transform을 제공하고, 재계산 한다.
   * ******/
   PullableObject pullable = GetComponent<PullableObject>();

   /**본들을 추가 및 제거한다....*/
   pullable.AddBone(tr[0]);
   pullable.AddBone(tr[1]);
   pullable.AddBone(tr[2]);
   pullable.AddBone(tr[3]);
   pullable.AddBone(tr[4]);
   pullable.AddBone(tr[5]);
   pullable.AddBone(tr[6]);
   pullable.AddBone(tr[7]);
   pullable.AddBone(tr[8]);
   pullable.RemoveBone(7);

   /**본들에 대한 정보를 재계산한다....*/
   pullable.RecalculateBoneDatas();

```
( **#Example 1** : 줄을 구성하는 ```Transform```을 추가하고, ```PullableObject```에서 필요한 정보를 재계산한다. )

``` c#

private PullableObject _pullingTarget;

private IEnumerator Start()
{
   /*********************************
   *   줄을 잡고 뒤로 당긴다....
   * ******/

   /**줄을 잡는다...*/
   _pullingTarget              = GetComponent<PullableObject>();
   _pullingTarget.HoldingPoint = gameObject;


   /**5초간 뒤로 줄을 당긴다...*/
   float timeLeft  = 5f;
   float deltaTime = Time.deltaTime;
   do
   {
       transform.position -= (transform.forward * -2f * deltaTime);
       yield return null;
    }
    while((timeLeft-=Time.deltaTime)>0f);

    /**줄을 놓는다.*/
    _pullingTarget.HoldingPoint = null;
}

```
( **#Example 2** : 줄을 5초간 뒤로 당기다가 놓는다. )

