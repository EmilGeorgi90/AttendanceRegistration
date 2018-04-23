DevExpress.viz.currentTheme("generic.light");
function clickEvent(attendance, hoursOfWeek) {
    var pieChart = $("#pie").dxPieChart({
        palette: "bright",
        dataSource: [{
                elev: attendance,
                timer: hoursOfWeek
            },
            {
                elev: "max timer i ugen",
                timer: 4 * 7 - hoursOfWeek
            }],
        title: "frevær",
        margin: {
            bottom: 20
        },
        legend: {
            visible: false
        },
        animation: {
            enabled: false
        },
        resolveLabelOverlapping: types[0],
        "export": {
            enabled: true
        },
        series: [{
                argumentField: "elev",
                valueField: "timer",
                label: {
                    visible: true,
                    customizeText: function (arg) {
                        return arg.argumentText + " ( " + arg.percentText + ")";
                    }
                }
            }]
    }).dxPieChart("instance");
    $("#types").dxSelectBox({
        dataSource: types,
        value: types[0],
        onValueChanged: function (e) {
            pieChart.option("resolveLabelOverlapping", e.value);
        }
    });
}
var mydata = [];
$("#safeme").click(function () {
    $("table tbody tr").each(function () {
        var td = $('td', this);
        mydata.push({
            AttendanceId: $('input[name="id"]', td).val(),
            hours: $('input[name="Hours"]', td).val(),
            notes: $('input[name="noter"]', td).val()
        });
    });
    $.ajax({
        url: this.action,
        type: this.method,
        data: JSON.stringify(mydata),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            var $response = $(result);
            var oneval = $response.filter('#content').html();
            console.log(oneval);
            $("#content").html(oneval);
        },
        error: function (request) {
            console.log(request);
        }
    });
});
var keys = {};
function checkKey(tr) {
    e = event || window.event;
    if (e.keyCode === '38') {
        console.log(this.parentNode.rowIndex);
    }
    else if (e.keyCode === '40') {
        console.log(this.parentNode.rowIndex);
    }
    else if (e.keyCode === '37') {
        console.log(this.parentNode.rowIndex);
    }
    else if (e.keyCode === '39') {
        console.log(this.parentNode.rowIndex);
    }
}
function Edit() {
    $.post("Attendance/Edit", $(this).serialize(), function (data, response) {
    });
}
var types = ["shift", "hide", "none"];
