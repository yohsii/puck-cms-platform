function LogHelper() {
    var self = this;
    $(document).on("click", "li.node.logs>.expand", function (e) {
        var el = $(this);
        var parentListItem = el.parents("li:first");
        if (parentListItem.find("ul.machines").is(":visible")) {
            parentListItem.find("ul.machines").hide();
            el.removeClass("fa-chevron-down").addClass("fa-chevron-right");
        }
        else {
            parentListItem.find("ul.machines").show();
            el.removeClass("fa-chevron-right").addClass("fa-chevron-down");
        }
    });
    $(document).on("click", "li.node.logs ul.machines>li>.expand", function (e) {
        var el = $(this);
        var parentListItem = el.parents("li:first");
        if (parentListItem.find("ul.logs").is(":visible")) {
            parentListItem.find("ul.logs").hide();
            el.removeClass("fa-chevron-down").addClass("fa-chevron-right");
        }
        else {
            parentListItem.find("ul.logs").show();
            el.removeClass("fa-chevron-right").addClass("fa-chevron-down");
        }
    });
    cright.on("keyup", ".logsContainer input", function (e) {
        var time = cright.find(".logsContainer input.time").val()||"";
        var level = cright.find(".logsContainer input.level").val()||"";
        var message = cright.find(".logsContainer input.message").val()||"";
        var stackTrace = cright.find(".logsContainer input.stackTrace").val()||"";
        var filteredEntries = [];
        for (var i = 0; i < self.entries.length; i++) {
            var entry = self.entries[i];
            if (
                (!(time.trim()) || entry.Time.indexOf(time.trim()) > -1)
                && (!(level.trim()) || entry.Level.toLowerCase().indexOf(level.trim().toLowerCase()) > -1)
                && (!(message.trim()) || entry.Message.toLowerCase().indexOf(message.trim().toLowerCase()) > -1)
                && (!(stackTrace.trim()) || entry.StackTrace.toLowerCase().indexOf(stackTrace.trim().toLowerCase()) > -1)
            )
                filteredEntries.push(entry);
        }
        self.drawEntries(filteredEntries);
    });
    this.entries = [];
    this.showLog = function (machine,name) {
        getLog(machine, name, function (res) {
            if (!machine)
                machine = res.machine;
            if (!name)
                name = res.name;
            console.log("log", res);
            self.entries = res.entries;
            var container = cinterfaces.find(".logsContainer").clone();
            var dateStr = name.replace(".txt", "");
            var moment = window.moment(dateStr, "YYYY-MM-DD");
            container.find(".date").html(moment.format("dddd DD MMMM YYYY"));
            container.find(".machineName").html(machine);
            cright.html(container);
            for (var i = 0; i < res.entries.length; i++) {
                var entry = res.entries[i];
                entry.Time = entry.Date.substring(entry.Date.indexOf("T")+1);
            }
            self.drawEntries(res.entries);
        });
    }
    this.drawEntries = function (entries) {
        var container = cright.find(".logsContainer");
        var table = $("<table/>").addClass("table");
        for (var i = 0; i < entries.length; i++) {
            var entry = entries[i];
            var tbody = $("<tbody/>");
            var tr1 = $("<tr/>");
            tr1.append($("<td/>").html(entry.Time));
            tr1.append($("<td/>").html(entry.Level));
            tr1.append($("<td/>").html(entry.Message));
            var tr2 = $("<tr/>");
            tr2.append($("<td colspan=\"3\"/>").html(entry.StackTrace.replace("\n", "<br/>")));
            table.append(tbody.append(tr1).append(tr2));
        }
        container.find("table").remove();
        container.append(table);
    }
    this.showMachines = function () {
        getLogMachines(function (res) {
            console.log("machines",res);
            var machinesListEl = $("<ul/>").addClass("machines").css({display:"none"});
            for (var i = 0; i < res.machines.length; i++) {
                var machine = res.machines[i];
                machinesListEl.append(
                    $("<li/>").attr({"data-machine":machine}).append("<i class=\"expand fas fa-chevron-right\" />"+machine)
                );
                self.showLogs(machine);
            }
            cleft.find(".left_developer li.logs").append(machinesListEl);
        });
    }
    this.showLogs = function (machine) {
        var logsListEl = $("<ul/>").addClass("logs").css({ display: "none" });
        getLogs(machine, function (res) {
            console.log("logs", res);
            for (var i = 0; i < res.logs.length; i++) {
                var log = res.logs[i].replace(".txt","");
                var listItem = $("<li>");
                listItem.append(
                    $("<a/>").attr({href:"#developer?page=logs&machine="+machine+"&name="+log}).html(log)
                );
                logsListEl.append(listItem);
            }
            cleft.find("li[data-machine='" + machine + "']").append(logsListEl);
            cleft.find(".left_developer a").removeClass("current");
            cleft.find(".left_developer a[href='" + location.hash + "']").addClass("current");
        });
    }

}