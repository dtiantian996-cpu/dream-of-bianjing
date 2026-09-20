using System.Collections.Generic;
using UnityEngine;

//这个表示 我们右键文件夹 Create/DataConfig/WeatherDataConfig 可以生成一个文件为WeatherDataConfig数据文件，我们的数据配置就是在这个文件下
[CreateAssetMenu(menuName = "DataConfig/WeatherDataConfig", fileName = "WeatherDataConfig")]
public class WeatherDataConfig : ScriptableObject
{
    public List<WeatherData> weatherData;
}