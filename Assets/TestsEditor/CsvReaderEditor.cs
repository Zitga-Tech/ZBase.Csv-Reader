using System;
using Csv;
using CsvReader;
using Newtonsoft.Json;
using NUnit.Framework;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

public class CsvReaderEditorTest
{
    private const string ASSET_PATH = "Assets/Samples/ScriptableObject/{0}.asset";

    private const string ASSET_PATH_VALIDATE = "Assets/Samples/ScriptableObject-Validate/{0}.asset";

    // A Test behaves as an ordinary method
    [Test]
    public void CsvReaderEditorTestAbilityDataConfig()
    {
        Assert.IsTrue(CompareTwoClass<AbilityDataConfig>());
    }

    [Test]
    public void CsvReaderEditorTestHeroLevelReward()
    {
        Assert.IsTrue(CompareTwoClass<HeroLevelReward>());
    }

    [Test]
    public void CsvReaderEditorTestRatingDataList()
    {
        Assert.IsTrue(CompareTwoClass<RatingDataList>());
    }
    
    [Test]
    public void CsvReaderEditorTestRatingDataListCustomPrimitiveArray()
    {
        Assert.IsTrue(CompareTwoClass<RatingDataListCustomPrimitiveArray, RatingDataList>(typeof(RatingDataListCustomPrimitiveArray).FullName, typeof(RatingDataList).FullName));
    }

    [Test]
    public void CsvReaderEditorTestRewardInfoList()
    {
        Assert.IsTrue(CompareTwoClass<RewardInfoList>());
    }

    [Test]
    public void CsvReaderEditorTestStageInfoConfig()
    {
        Assert.IsTrue(CompareTwoClass<StageInfoConfig>());
    }

    [Test]
    public void CsvReaderEditorTestStageReward()
    {
        Assert.IsTrue(CompareTwoClass<StageReward>());
    }

    [Test]
    public void CsvReaderEditorTestHeroConfig()
    {
        Assert.IsTrue(CompareTwoClass<HeroConfig>());
    }
    
    [Test]
    public void CsvReaderEditorTestApiConfig()
    {
        Assert.IsTrue(CompareTwoClass<ApiConfig>());
    }
    
     
    [Test]
    public void CsvReaderEditorTestBigNumber()
    {
        Assert.IsTrue(CompareTwoClass<FormatBigNumber>());
    }

    [Test]
    public void CsvReaderEditorTestConverterTypeArguments()
    {
        var type = typeof(CustomIntConverter);
        var typeArgs = type.GetArgumentsOfInheritedOpenGenericInterface(typeof(IConvert<,>));
        Assert.AreEqual(typeArgs.Length, 2);
        Assert.AreEqual(typeArgs[0], typeof(int));
        Assert.AreEqual(typeArgs[1], typeof(CustomInt));
    }

    [Test]
    public void CsvReaderEditorTestZombieConfig()
    {
        var testZombieIds = new [] {2001, 2002};
        var isTrue = true;
        foreach (var testZombieId in testZombieIds)
        {
            var resultCompare = CompareTwoClass<ZombieConfig>($"{typeof(ZombieConfig).FullName}_{testZombieId}");
            if (!resultCompare)
            {
                isTrue = false;
                break;
            }
        }
        Assert.IsTrue(isTrue);
    }


    private static bool CompareTwoClass<T>(string inputFullName = "") where T : ScriptableObject
    {
        var fullName = string.IsNullOrEmpty(inputFullName) ? typeof(T).FullName : inputFullName;
        var validateData = AssetDatabase.LoadAssetAtPath<T>(string.Format(ASSET_PATH, fullName));
        var newData = AssetDatabase.LoadAssetAtPath<T>(string.Format(ASSET_PATH_VALIDATE, fullName));

        return JsonCompare(fullName, validateData, newData);
    }

    private static bool CompareTwoClass<TU, TV>(string fullNameU, string fullNameV) where TU : ScriptableObject where TV : ScriptableObject
    {
        var newData = AssetDatabase.LoadAssetAtPath<TU>(string.Format(ASSET_PATH, fullNameU));
        var validateData = AssetDatabase.LoadAssetAtPath<TV>(string.Format(ASSET_PATH_VALIDATE, fullNameV));

        if (fullNameV != null)
        {
            newData.name = fullNameV;
        }

        var result = JsonCompare(fullNameU, newData, validateData, true);

        if (fullNameU != null)
        {
            newData.name = fullNameU;
        }

        return result;
    }

    private static bool JsonCompare(string className, object obj, object another, bool isIgnoreType = false)
    {
        if (ReferenceEquals(obj, another))
        {
            Debug.Log($"The same reference: {className}");
            return true;
        }

        if ((obj == null) || (another == null))
        {
            Debug.Log($"Object is null: {className}  o1[{obj}] o2[{another}]");
            return false;
        }

        if (isIgnoreType == false && obj.GetType() != another.GetType())
        {
            Debug.Log($"Not the same type: o1[{obj.GetType()}] o2[{another.GetType()}]");
            return false;
        }
        
        var objJson = JsonConvert.SerializeObject(obj);
        var anotherJson = JsonConvert.SerializeObject(another);

        var result = objJson == anotherJson;

        if (result == false)
        {
            Debug.Log($"Not the same value: \n OBJ:\n{objJson}\nANOTHER:\n{anotherJson}");
        }

        return result;
    }

    [Serializable]
    public struct CustomInt
    {
        public int value;

        public static implicit operator CustomInt(int value)
        {
            return new CustomInt { value = value };
        }

        public static implicit operator int(CustomInt value)
        {
            return value.value;
        }
    }

    public readonly struct CustomIntConverter : IConvert<int, CustomInt>
    {
        public CustomInt Convert(object value)
        {
            return (int)value;
        }
    }
}