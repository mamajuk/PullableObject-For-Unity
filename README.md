# PullableObject
## Overview
```PullableObject```는 시작 위치가 고정된 줄의 형태를 이루는 연속적인 ```Transform```을 조작하여, 줄이 펴지거나 끊어지는 움직임이 구현된 컴포넌트입니다. **Unity Editor Inspector** 에서 줄의 형태를 이루는 연속적인 ```Transform``` 을 조작하여 손쉽게 임의의 복잡하게 꼬여있는 줄을 구성할 수 있는 인터페이스를 제공하며 스크립팅 레벨 또는 인스펙터에서 줄의 상태에 따른 유연한 확장을 할 수 있도록 줄이 당겨지기 **시작했을 때**, **끊어졌을 때**, **펴졌을 때** 와 같은 줄의 상태 변화시에 호출되는 각종 대리자를 제공합니다. 
만약 메시의 본들의 ```Transform``` 으로 줄을 구성하였을 경우, 메시가 꼬이는 ```캔디랩( Candy wrap )``` 현상이 발생할 수 있음을 고려해야 하며, 별도의 메시없이 줄을 구성하는 연속적인 ```Transform```만 사용할 경우 ```LineRenderer```를 ```PullableObject```에 제공하여 제공된 ```Transform```을 기반으로하는 선을 랜더링하는 사용법을 선택할 수 있습니다. 다음은 ```PullableObject```를 적용한 에시를 보여줍니다.
<table><tr><td>
<img width="500px" img src="https://private-user-images.githubusercontent.com/52849917/286435226-1f6fece8-a7f6-45f4-97b9-301f752d2c4b.gif?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTEiLCJleHAiOjE3MDEyMTcwODAsIm5iZiI6MTcwMTIxNjc4MCwicGF0aCI6Ii81Mjg0OTkxNy8yODY0MzUyMjYtMWY2ZmVjZTgtYTdmNi00NWY0LTk3YjktMzAxZjc1MmQyYzRiLmdpZj9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFJV05KWUFYNENTVkVINTNBJTJGMjAyMzExMjklMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjMxMTI5VDAwMTMwMFomWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPWYyYzJlNTIxNGM0ZGU1ZTU3MjJhZDQ2M2JjYWM3OWRlNGI3MTJiZDZhYjkyNjZjZWQwMzljMTNmNGU0ZmZhNTYmWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0JmFjdG9yX2lkPTAma2V5X2lkPTAmcmVwb19pZD0wIn0.EUm7LzqcQOfTYf38dIjVw9DoqNBFfPFbJXg1W6n9Fx0">
</td></tr></table>

## Tutorial
```PullableObject``` 를 적용하기 위해서 가장 먼저 해야할 일은 ```PullableObject``` 를 적용할 줄을 구성하는 메시의 본들/게임 오브젝트들의 ```Transform```을 ```PullableObject``` 에게 제공하는 것입니다. 다음은 **Unity Editor Inspector** 에서 줄의 ```Transform```을 **Datas** 컨테이너에 제공하는 것을 보여줍니다.

<table><tr><td>
<img width="400px" img src="https://www.notion.so/image/https%3A%2F%2Fprod-files-secure.s3.us-west-2.amazonaws.com%2F4a0956e0-5579-46a0-b3e2-a74896f5ae67%2F54ff6581-6735-4f34-9472-c7f5a51679b9%2FUntitled.png?table=block&id=bc35f7cf-b246-427f-bb13-f3295cd61bb5&spaceId=4a0956e0-5579-46a0-b3e2-a74896f5ae67&width=880&userId=40a1489e-b817-44b0-9900-e95ad958047a&cache=v2">
</td></tr></table>

적절히 **Datas** 컨테이너를 채워넣은 후, ```HoldingPoint``` 에 당겨지는 위치를 나타내는 ```GameObject```를 제공함으로써 당겨지는 효과를 적용할 수 있습니다. 더 자세한 내용 및 사용법은  [PullableObject Reference](https://bramble-route-61a.notion.site/Unity-C-PullableObject-07d6fa3a84aa4084aab64114ec633d18?pvs=4) 를 참조하세요.
