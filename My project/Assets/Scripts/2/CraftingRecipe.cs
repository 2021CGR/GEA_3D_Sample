using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작 레시피 데이터:
/// - 입력(Ingredient) 목록과 출력(Product) 목록으로 구성
/// - ScriptableObject로 프로젝트/인스펙터에서 손쉽게 생성/편집
/// </summary>
[CreateAssetMenu(fileName = "Recipe", menuName = "조합법 생성")]
public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public struct Ingredient
    {
        public BlockType type;
        public int count;
    }

    [System.Serializable]
    public struct Product
    {
        public BlockType type;
        public int count;
    }

    public string displayName;
    public List<Ingredient> inputs = new List<Ingredient>();
    public List<Product> outputs = new List<Product>();
}
