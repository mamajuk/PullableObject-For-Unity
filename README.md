# PullableObject
## Overview
```PullableObject```는 시작 위치가 고정된 줄의 형태를 이루는 연속적인 ```Transform```을 조작하여, 줄이 펴지거나 끊어지는 움직임이 구현된 컴포넌트입니다. **Unity Editor Inspector** 에서 줄의 형태를 이루는 연속적인 ```Transform``` 을 조작하여 손쉽게 임의의 복잡하게 꼬여있는 줄을 구성할 수 있는 인터페이스를 제공합니다.
스크립팅 레벨 또는 인스펙터에서 줄의 상태에 따른 유연한 확장을 할 수 있도록 줄이 당겨지기 **시작했을 때**, **끊어졌을 때**, **펴졌을 때** 와 같은 줄의 상태 변화시에 호출되는 각종 대리자를 제공합니다. ```PullableObject``` 는 런타임에 별도로 줄을 표시하는 기능을 제공하지 않으며, 단순히 줄을 구성하는 ```Transform``` 들을 조작하는 기능만을 제공합니다. 만약 메시의 본들의 ```Transform``` 으로 줄을 구성하였을 경우, 메시가 꼬이는 ```캔디랩( Candy wrap )``` 현상이 발생할 수 있음을 고려하여야 합니다. 

## Tutorial
```PullableObject``` 를 적용하기 위해서 가장 먼저 해야할 일은 ```PullableObject``` 를 적용할 줄을 구성하는 메시의 본들/게임 오브젝트들의 ```Transform```을 ```PullableObject``` 에게 제공하는 것입니다. 다음은 **Unity Editor Inspector** 에서 줄의 ```Transform```을 **Datas** 컨테이너에 제공하는 것을 보여줍니다.

<table><tr><td>
<img width="400px" img src="https://www.notion.so/image/https%3A%2F%2Fprod-files-secure.s3.us-west-2.amazonaws.com%2F4a0956e0-5579-46a0-b3e2-a74896f5ae67%2F0285ed9f-1ca5-4a34-bef7-0836ba62da8a%2FUntitled.png?table=block&id=baabdfe4-d406-4823-a9f5-91d7706849df&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=830&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

적절히 **Datas** 컨테이너를 채워넣은 후, ```HoldingPoint``` 에 당겨지는 위치를 나타내는 ```GameObject```를 제공함으로써 당겨지는 효과를 적용할 수 있습니다. 더 자세한 내용 및 사용법은  [PullableObject Reference](https://bramble-route-61a.notion.site/Unity-C-PullableObject-07d6fa3a84aa4084aab64114ec633d18?pvs=4) 를 참조하세요.
