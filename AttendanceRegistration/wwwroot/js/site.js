DevExpress.viz.currentTheme("generic.light");
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
$(document).on("click", ".fullname", function () {
    var value = 0;
    var array = $(this).nextUntil(".fullname").children()
    var array2 = $(this).parent().prev().children().children(".row").children()
    var json = [{
        val: value,
        hours: value
    },
        {
            }]
    array.each(function (index) { value += ($(this).children()[0].value) })
    console.log(json);
    var viewModel = {
        chartOptions: {
            size: {
                width: 500
            },
            palette: "bright",
            dataSource: value,
            series: [
                {
                    argumentField: 7*4,
                    valueField: value,
                    label: {
                        visible: true,
                        connector: {
                            visible: true,
                            width: 1
                        }
                    }
                }
            ],
            title: "frevær",
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