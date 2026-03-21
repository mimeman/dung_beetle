using UnityEngine;

#region [ItemType 설명]
/* [설명]
 * 아이템의 대분류를 정의하는 열거형입니다.
 * 기획서에 따라 소모형(식량, 엽전, 목재, 약초)과 비소모형(도구, 의뢰)으로 구분됩니다.
 * 아이템의 종류에 따라 UI에서 표시될 고유 색상 등을 결정하는 기준이 됩니다.
 */
#endregion

public enum ItemType
{
    Food,          // 식량 (곡물, 고기, 물고기)
    Currency,      // 엽전 (냥 단위)
    Repair,        // 수리 자재 (목재)
    Recovery,      // 회복 아이템 (약초)
    Tool,          // 도구 (곡괭이 등)
    Quest          // 비소모성/의뢰 아이템
}