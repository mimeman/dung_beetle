using Dung.Enums;

namespace Dung.Data.Save 
{
    // 쇠똥의 상태를 저장하기 위한 순수 데이터 클래스
    [System.Serializable]
    public class DungSaveData
    {
        // [식별자]
        public string id;

        // [물리 정보]
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;

        // [쇠똥 상태]
        public float currentMass;
    }
}