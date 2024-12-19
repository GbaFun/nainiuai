
function doTest() {
    var data = MERGE_Form({
        eid: 1, rune: 33, count: 20,
    });
    console.log(data);

    GET_JSON_EquipName("死灵", 7, "主手", function (equipName) {
        console.log(equipName)
    });
    GET_JSON_EquipName("死灵", 14, "戒指1", function (equipName) {
        console.log(equipName)
    });
    GET_JSON_EquipName("武僧", 4, "头盔", function (equipName) {
        console.log(equipName)
    });
    GET_JSON_EquipName("武僧", 11, "靴子", function (equipName) {
        console.log(equipName)
    });

}
