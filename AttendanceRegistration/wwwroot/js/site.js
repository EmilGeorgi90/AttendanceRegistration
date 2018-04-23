DevExpress.viz.currentTheme("generic.light");
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
    $.post("Attendance/Edit", $(this).serialize(),
        function (data, response) {
        });
}
var options = {
    day: "numeric", month: "numeric", year: "numeric"
};
function Attendance(attendanceId, hours, dates, datesId, user, userId) {
    var self = this;
    self.attendanceId = attendanceId;
    self.hours = hours;
    self.dates = dates;
    self.datesId = datesId;
    self.user = user;
    self.userId = userId;
}

//single Spread containing an Observable Array of Keys (that contain Observable Arrays with Observable data)
function Person(id, fullname, attendances, notes) {
    var self = this;
    self.id = id;
    self.fullname = fullname;
    self.notes = notes;
    self.attendances = ko.observableArray(ko.utils.arrayMap(attendances, function (attendance) {
        return new Attendance(attendance.AttendanceId, attendance.Hours, new Date(attendance.Dates.ShcoolData).toLocaleString("DK", options), attendance.DatesId, attendance.User.Fullname, attendance.UserId);
    }));
}

// Overall viewmodel for this screen, along with initial state

function ForecastDetailCashFlowViewModel(person) {
    var self = this;
    self.persons = ko.utils.arrayMap(person, function (person) {
        return new Person(person.Id, person.Fullname, person.Attendances, person.Notes);
    });
}
var json = document.getElementById("json").innerHTML;
var mydata = JSON.parse(json);
$(function () {
    ko.applyBindings(new ForecastDetailCashFlowViewModel(mydata), document.getElementById("data"));
});
var something = new ForecastDetailCashFlowViewModel(mydata).persons;
var aids = new something[0].attendances;
var hours = aids[0].hours;
console.log(something);
$(document).on("click", ".fullname" ,function () {
    console.log($(this));
    var viewModel = {
        chartOptions: {
            size: {
                width: 500
            },
            palette: "bright",
            dataSource: aids,
            series: [
                {
                    argumentField: aids.user,
                    valueField: "hours",
                    label: {
                        visible: true,
                        connector: {
                            visible: true,
                            width: 1
                        }
                    }
                }
            ],
            title: "Area of Countries",
            "export": {
                enabled: true
            },
            onPointClick: function (e) {
                var point = e.target;

                toggleVisibility(point);
            },
            onLegendClick: function (e) {
                var arg = e.target;

                toggleVisibility(e.component.getAllSeries()[0].getPointsByArg(arg)[0]);
            }
        }
    };

    function toggleVisibility(item) {
        if (item.isVisible()) {
            item.hide();
        } else {
            item.show();
        }
    }

    ko.applyBindings(viewModel, document.getElementById("chart-demo"));
});