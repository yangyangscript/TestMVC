
//常用 information(详细) pencil(编辑) delete(删除)
//GirdConfig arrow_up(上移) arrow_down(下移) arrow_refresh(改变专题)
//其他 picture 
function renderAction(keyStr,titleStr) {
    var iconStr = "";
    switch (keyStr) {
        case "view":
            iconStr = "information";
            break;
        case "info":
            iconStr = "information";
            break;
        case "edit":
            iconStr = "pencil";
            break;
        case "delete":
            iconStr = "delete";
            break;

        case "up":
            iconStr = "arrow_up";
            break;

        case "down":
            iconStr = "arrow_down";
            break;

        case "change":
            iconStr = "arrow_refresh";
            break;
        default:
            iconStr = "";
            break;
    }
    //var itemHtml = F.formatString('<li><span data-qtip="{0}">', item.name);
    if (iconStr == "") return "";
    return F.formatString('<a class="action-btn {0}" href="javascript:;" title="{1}"><img class="f-grid-cell-icon" src="/res/icon/{2}.png"></a>', keyStr, titleStr, iconStr);
};
function getGridConfig(gridId) {
    var gird = F(gridId);
    var ret = {
        "fields": gird.fields,
        "IsPaging": gird.paging,
        "IsSorting": gird.sorting,
        "PageSize": gird.pageSize,
        "PageIndex": gird.pageIndex,
        "SortField": gird.sortField,
        "SortDirection": gird.sortDirection
    };
    return F.toJSON(ret);
};
function selectAll(gridId) {
    var grid = F(gridId);
    grid.deselectAllRows();
};