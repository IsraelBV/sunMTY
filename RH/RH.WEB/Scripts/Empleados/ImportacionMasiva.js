$(document).ready(function () {
    $("#fileInput").change(function () {
        var data = new FormData();
        var files = $("#fileInput").get(0).files;
        if(files.length > 0)
            data.append("UploadedImage", files[0])

        var ajaxRequest = $.ajax({
            xhr: function() {
                var xhr = new window.XMLHttpRequest();
                $(".panel-body").show();
                xhr.addEventListener('progress', function (e) {
                    if (e.lengthComputable) {
                        $(".progress-bar").css("width", " " + (100 * e.loaded / e.total) + "%");
                    }
                });
                return xhr;
            },
            type: "POST",
            url: "UploadFile",
            contentType: false,
            processData: false,
            data: data
        });

        ajaxRequest.done(function (data, xhr) {
            $("#records").html(data);
            if ($("#countRecords").html() > 0) {
                $("#btnProcesar").attr("disabled", false);
            }
        });
    });

    $("#btnProcesar").click(function () {
        $("#btnProcesar").attr("disabled", true);
        window.location.href = "/Empleados/UploadRecords";
    });
});