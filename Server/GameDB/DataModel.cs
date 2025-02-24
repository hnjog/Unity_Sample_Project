using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDB
{
    // 데이터 모으는 묶음
    // Clusterredvs NonClustered
    // Clusterred
    // : 데이터가 실제 테이블의 물리적 순서와 인덱스 순서가 동일하게 저장
    //   각 테이블에는 하나의 클러스터 인덱스만 존재 가능
    //   인덱스를 통해 데이터에 직접 접근하므로 속도가 빠름
    //   (다만 데이터 삽입,수정,삭제 등에서 물리적 위치를 변경해야 할 수 있어 성능저하 발생 가능)
    // 
    // NonClustered
    // : 데이터가 테이블 순서와 관계없이 별도의 구조로 저장
    //   여러 개의 논클러스터 인덱스 생성 가능
    //   인덱스가 데이터에 대한 포인터를 포함하므로, 실제 데이터를 찾기 위해 추가적인 작업 필요
    //   다양한 쿼리 적용 가능, 유연성이 높음
    //   물리적 위치는 그대로 이기에 삽입, 수정, 삭제 등으로 성능저하 발생 x
    //   일반적으로는 클러스터링 방식보단 성능이 느림
    //   범위 검색 시, 추가적인 성능 저하가 발생 가능
    //
    [Table("Test")]
    public class TestDb
    {
        // Convention : [클래스]Id로 명명하면 Priority Key
        // PK로 찾아야 가장 빠르게 답을 찾을 수 있음
        public int TestDbId { get; set; }
        public string Name { get; set; }
    }
}
