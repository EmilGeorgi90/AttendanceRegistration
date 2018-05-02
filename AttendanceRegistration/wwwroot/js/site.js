DevExpress.viz.currentTheme("generic.light");
var options = {
    month: "numeric",
    day: "numeric",
    year: "numeric"
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
//the constructer to person class
function PersonViewModel(person) {
    var self = this;
    self.persons = ko.utils.arrayMap(person, function (person) {
        return new Person(person.Id, person.Fullname, person.Attendances, person.Notes);
    });
}
//getting json from browser/controller
var json = document.getElementById("json").innerHTML;

var mydata = JSON.parse(json);
console.log(mydata)

$(function () {
    //bind the person data
    ko.applyBindings(new PersonViewModel(mydata), document.getElementById("data"));
});
//the pie creater
$(document).on("click", ".fullname", function () {
    ko.cleanNode(document.getElementById("chart-demo"));
    var value = 0;
    var jsonSemesterHT = [JSON.parse(document.getElementById("JSONSemester").innerHTML)];
    var jsonWeekHT = [JSON.parse(document.getElementById("JSONWeek").innerHTML)];
    var jsonModulHT = [JSON.parse(document.getElementById("JSONSModul").innerHTML)];
    console.log(jsonWeekHT[0].length)
    console.log(jsonModulHT[0].length)
    console.log(jsonSemesterHT[0].length)
    //getting all hours
    var array = $(this).nextUntil(".fullname").children(".hours");
    //getting username
    var array2 = $(this).parent().prev().children().children(".row").children();
    var some = array2[0].innerText.split('/');
    var array3 = [];
    //each function
    $(some).each(function (index, element) {
        if (index === 0) {
            array3.push(element);
        } else if (index === 1) {
            array3.unshift(element);
        } else if (index === 2) {
            array3.push(element);
        }
    });
    var weekIsEvenOrOdd;
    if (weekIsEven(array3.join('/'))) {
        weekIsEvenOrOdd = 7 * 5;
    } else {
        weekIsEvenOrOdd = 7 * 4;
    }
    //pluses value 
    array.each(function (index) {
        if (index > jsonWeekHT[0].length) {
            return;
        }
        if ($(this).children().prop("tagName") === "DIV") {
            value += parseInt($(this).children()[0].innerText);
        } else {
            value += parseInt($(this).children()[0].value);
        }
    });
    var jsonWeek = [{
        name: $(this)[0].innerText,
        hours: value
    }, {
        name: "hours of week",
        hours: weekIsEvenOrOdd
    }];
    value = 0;
    if (weekIsEven(array3.join('/'))) {
        weekIsEvenOrOdd = 7 * jsonModulHT[0].length;
    } else {
        weekIsEvenOrOdd = 7 * jsonModulHT[0].length;
    }
    array.each(function (index) {
        if (index > jsonModulHT[0].length) {
            return;
        }
        if ($(this).children().prop("tagName") === "DIV") {
            value += parseInt($(this).children()[0].innerText);
        } else {
            value += parseInt($(this).children()[0].value);
        }
    });
    var jsonModul = [{
        name: $(this)[0].innerText,
        hours: value
    }, {
        name: "hours of modul",
        hours: weekIsEvenOrOdd
    }];
    if (weekIsEven(array3.join('/'))) {
        weekIsEvenOrOdd = 7 * jsonSemesterHT[0].length;
    } else {
        weekIsEvenOrOdd = 7 * jsonSemesterHT[0].length;
    }
    value = 0;

    array.each(function (index) {
        if (index > jsonSemesterHT[0].length) {
            return;
        }
        if ($(this).children().prop("tagName") === "DIV") {
            value += parseInt($(this).children()[0].innerText);
        } else {
            value += parseInt($(this).children()[0].value);
        }
    });
    var jsonSemester = [{
        name: $(this)[0].innerText,
        hours: value
    }, {
        name: "hours of semester",
        hours: weekIsEvenOrOdd
    }];

    viewModelWeek = {
        chartOptions: {
            palette: "bright",
            dataSource: jsonWeek,
            series: [{
                argumentField: "name",
                valueField: "hours",
                label: {
                    visible: true,
                    customizeText: function (arg) {
                        return arg.argumentText + " ( " + arg.percentText + ")";
                    }
                }
            }],
            title: "frevær på ugen",
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
    viewModelModul = {
        chartOptions: {
            palette: "bright",
            dataSource: jsonModul,
            series: [{
                argumentField: "name",
                valueField: "hours",
                label: {
                    visible: true,
                    customizeText: function (arg) {
                        return arg.argumentText + " ( " + arg.percentText + ")";
                    }
                }
            }],
            title: "frevær på modulet",
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
    viewModelSemester = {
        chartOptions: {
            palette: "bright",
            dataSource: jsonSemester,
            series: [{
                argumentField: "name",
                valueField: "hours",
                label: {
                    visible: true,
                    customizeText: function (arg) {
                        return arg.argumentText + " ( " + arg.percentText + ")";
                    }
                }
            }],
            title: "frevær på semesteret",
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
    ko.applyBindings(viewModelWeek, document.getElementById("chart-demo"));
    ko.applyBindings(viewModelModul, document.getElementById("chart-Modul"));
    ko.applyBindings(viewModelSemester, document.getElementById("chart-Semester"));

});
//edit function for the teacher to edit the students hours
$(document).on("focusout", "#hoursEdit", function () {
    var dataJson = new PersonViewModel(mydata);
    var json = document.getElementById("json").innerHTML;
    var Attendanceid = JSON.parse(json);
    for (var i = 0; i < Attendanceid.length; i++) {
        if (Attendanceid[i].Fullname === $(this).parent().parent().prev(".fullname")[0].innerText) {
            var dataToServer = {
                hours: parseInt($(this).val()),
                attendanceId: $(this).data("prop")
            };
            $.post("Attendance/Edit", dataToServer);
        }
    }
});
//getting week of the year
function WeekOfYeah(dateOfYeah) {
    var onejan = new Date(dateOfYeah);
    onejan.setUTCDate(onejan.getUTCDate() + 4 - (onejan.getUTCDay() || 7));
    var yearStart = new Date(Date.UTC(onejan.getUTCFullYear(), 0, 1));
    return Math.ceil((((onejan - yearStart) / 86400000) + 1) / 7);
}

//checking if the week is even and throw exception if it is not a date
function weekIsEven(dateOfYeah) {
    if (WeekOfYeah(dateOfYeah) % 2 === 1) {
        return false;
    } else if (WeekOfYeah(dateOfYeah) % 2 === 0) {
        return true;
    } else {
        throw new DOMException;
    }
}
$(function () {
    $("#draggable").draggable({
        axis: "x"
    });
})