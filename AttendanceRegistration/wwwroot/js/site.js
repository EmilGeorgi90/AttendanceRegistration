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
$(document).on("click", ".fullname", function () {
    
    var value = 0;
    var array = $(this).nextUntil(".fullname").children()
    var array2 = $(this).parent().prev().children().children(".row").children()
    console.log(data.prototype.getWeekYear);
    array.each(function (index) { value += parseInt($(this).children()[0].value) })
    var json = [{
        name: $(this)[0].innerText,
        hours: value / 28 * 100
    }, {
            name: "hours of week",
            hours: 28
        }]
    var viewModel = {
        chartOptions: {
            size: {
                width: 500
            },
            palette: "bright",
            dataSource: json,
            series: [
                {
                    argumentField: "name",
                    valueField: "hours",
                    label: {
                        visible: true,
                        customizeText: function (arg) {
                            return arg.argumentText + " ( " + arg.percentText + ")";
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
Date.prototype.getWeek = function () {
    var date = new Date(this.getTime());
    date.setHours(0, 0, 0, 0);
    // Thursday in current week decides the year.
    date.setDate(date.getDate() + 3 - (date.getDay() + 6) % 7);
    // January 4 is always in week 1.
    var week1 = new Date(date.getFullYear(), 0, 4);
    // Adjust to Thursday in week 1 and count number of weeks from date to week1.
    return 1 + Math.round(((date.getTime() - week1.getTime()) / 86400000
        - 3 + (week1.getDay() + 6) % 7) / 7);
}

// Returns the four-digit year corresponding to the ISO week of the date.
Date.prototype.getWeekYear = function () {
    var date = new Date(this.getTime());
    date.setDate(date.getDate() + 3 - (date.getDay() + 6) % 7);
    return date.getFullYear();
}