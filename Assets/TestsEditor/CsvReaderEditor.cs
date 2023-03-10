using Csv;
using Newtonsoft.Json;
using NUnit.Framework;
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
        Assert.IsTrue(CompareTwoClass<RatingDataListCustomPrimitiveArray, RatingDataList>());
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


    private static bool CompareTwoClass<T>() where T : ScriptableObject
    {
        var fullName = typeof(T).FullName;
        var validateData = AssetDatabase.LoadAssetAtPath<T>(string.Format(ASSET_PATH, fullName));
        var newData = AssetDatabase.LoadAssetAtPath<T>(string.Format(ASSET_PATH_VALIDATE, fullName));

        return JsonCompare(fullName, validateData, newData);
    }

    private static bool CompareTwoClass<TU, TV>() where TU : ScriptableObject where TV : ScriptableObject
    {
        var fullNameU = typeof(TU).FullName;
        var fullNameV = typeof(TV).FullName;
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
}