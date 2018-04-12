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
var mydata = {
    AttendanceId: $(this).data("id"),
    Hours: this.innerHTML,
    Note: $(this).data("notes")
}
$(document).ready(function () {
    $(".notes").focusout(function () {
        $.post("Attendance/Edit",
            {
                AttendanceId: $(this).data("id"),
                hours: $(this).data("Hours"),
                notes: this.innerHTML
            },
            function (data, response) {
                $("body").html(data)
            })
    })
    $(".hours").focusout(function () {
        $.post("Attendance/Edit",
            {
                AttendanceId: $(this).data("id"),
                hours: this.innerHTML,
                notes: $(this).data("notes")
            },
            function (data, response) {
                $("body").html(data)
            })
    })
})
function Edit() {
    $.post("Attendance/Edit", $(this).serialize(),
        function (data, response) {
        })
}


var types = ["shift", "hide", "none"];