function initQA(serverID) {
    if ($("#QADiv") != null) {
        var serverName = "";
        function updateQA() {
            $.post("/API/Server/GetServer",
                {
                    ServerID: serverID
                },
                function (data, status) {
                    var root = $($.parseXML(data)).find("Response");

                    if (root.children("Type").text() == "Error") {
                        toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                        return;
                    }

                    serverName = root.children("Content").children("Name").text();

                    var CanStart = root.children("Content").children("CanStart").text() == "true";
                    var CanStop = root.children("Content").children("CanStop").text() == "true";
                    var CanRestart = root.children("Content").children("CanRestart").text() == "true";
                    var CanKill = root.children("Content").children("CanKill").text() == "true";

                    $("#QADiv").hide();
                    if (CanStart || CanStop || CanRestart || CanKill) {
                        $("#QADiv").show();

                        $("#btnQAStart").hide();
                        $("#btnQAStop").hide();
                        $("#btnQARestart").hide();
                        $("#btnQAKill").hide();

                        var state = root.children("Content").children("State").text();

                        if (CanStart && (state == "Unknow" || state == "Stopped")) {
                            $("#btnQAStart").show();
                        }
                        if (CanStop && (state == "Unknow" || state == "Running")) {
                            $("#btnQAStop").show();
                        }
                        if (CanRestart && (state == "Unknow" || state == "Running")) {
                            $("#btnQARestart").show();
                        }
                        if (CanKill && (state == "Unknow" || state == "Starting" || state == "Running" || state == "Stopping")) {
                            $("#btnQAKill").show();
                        }
                    }
                });
        }

        setInterval(updateQA, 10000);
        updateQA();

        $("#btnQAStart").click(function () {
            bootbox.confirm("Are you sure you want to start this server ?",
            function (result) {
                if (result) {
                    $.post("/API/Server/StartServers/",
                    {
                        ServerIDs: serverID
                    },
                    function (data, status) {
                        var root = $($.parseXML(data)).find("Response");

                        if (root.children("Type").text() == "Error") {
                            toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                            return;
                        }
                        toastr.success(serverName + " has been started", "Server Started");
                    });

                }
            });
        });

        $("#btnQAStop").click(function () {
            bootbox.confirm("Are you sure you want to stop this server ?",
            function (result) {
                if (result) {
                    $.post("/API/Server/StopServers/",
                    {
                        ServerIDs: serverID
                    },
                    function (data, status) {
                        var root = $($.parseXML(data)).find("Response");

                        if (root.children("Type").text() == "Error") {
                            toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                            return;
                        }
                        toastr.success(serverName + " has been stopped", "Server Stopped");
                    });
                }
            });
        });

        $("#btnQARestart").click(function () {
            bootbox.confirm("Are you sure you want to restart this server ?",
            function (result) {
                if (result) {
                    $.post("/API/Server/RestartServers/",
                    {
                        ServerIDs: serverID
                    },
                    function (data, status) {
                        var root = $($.parseXML(data)).find("Response");

                        if (root.children("Type").text() == "Error") {
                            toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                            return;
                        }
                        toastr.success(serverName + " has been restarted", "Server Restarted");
                    });
                }
            });
        });

        $("#btnQAKill").click(function () {
            bootbox.confirm("Are you sure you want to start this server ?",
            function (result) {
                if (result) {
                    $.post("/API/Server/KillServers/",
                    {
                        ServerIDs: serverID
                    },
                    function (data, status) {
                        var root = $($.parseXML(data)).find("Response");

                        if (root.children("Type").text() == "Error") {
                            toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                            return;
                        }
                        toastr.success(serverName + " has been killed", "Server Killed");
                    });
                }
            });
        });
    }
}