//------------------------------ GET PERIODOS PAGO ------------------------------------------------------>
$(document).on("tableUniselect:double-select", "#tblPeriodosPago", function () {
    var $row = $(this).find(".double-select");
    var IdPeriodo = $row.data("idperiodopago");
    utils.loadMainPage("PeriodosPago", "PeriodoDetalle", "#main-content", false, IdPeriodo);
});

$(document).on("tableUniselect:select", "#tblPeriodosPago", function () {
    $(".btnDetallePeriodo").attr("disabled", false);
});

$(document).on("tableUniselect:deselect", "#tblPeriodosPago", function () {
    $(".btnDetallePeriodo").attr("disabled", true);
});

$(document).on("click", ".btnDetallePeriodo", function () {
    var $row = $("#tblPeriodosPago").find(".double-select");
    if ($row.length < 1)
        $row = $("#tblPeriodosPago").find(".selected");

    if ($row.length > 0){
        var IdPeriodo = $row.data("idperiodopago");
        utils.loadMainPage("PeriodosPago", "PeriodoDetalle", "#main-content", false, IdPeriodo);
    }
    else 
        utils.showMessage("¡Algo salió mal!", "Por favor selecciona al menos un periodo de pago para consultar su detalle", 5000, "error");
});

//------------------------------ FIN PERIODOS PAGO ------------------------------------------------------>

//------------------------------CREAR PERIODOS DE PAGO -------------------------------------------------->

function OnSuccessCrearPeriodo(data) {
    if(data > 0) { //data = IdPeriodoPago
        utils.loadMainPage("PeriodosPago", "SeleccionEmpleadosPeriodo", "#main-content", true, data);
    }
    else {
        utils.showMessage("Algo salió mal - COD ERR:" + data.code, data.message, 5000, "info");
    }
}

$(document).on("change", "select[name='IdTipoNomina']", function () {
    var idtiponomina = $(this).val();
    if (idtiponomina != 12) {
        var dias = parseInt($(this).find(":selected").data("numDias"));
        $("input[name='DiasPeriodo']").val(dias);
    }

});

$(document).on("keyup", "input[name='DiasPeriodo']", function () {
    var $DiasPeriodo = $(this).val();
    var $FechaInicio = $("input[name='Fecha_Inicio']").val();

    if ($DiasPeriodo != "" && $FechaInicio != "") {
        calcularDiasPeriodo($DiasPeriodo, $FechaInicio);
    }
});

$(document).on("change", "input[name='Fecha_Inicio']", function () {
    var $FechaInicio = $(this).val();
    var $DiasPeriodo = $("input[name='DiasPeriodo'").val();

    if ($DiasPeriodo != "" && $FechaInicio != "") {
        calcularDiasPeriodo($DiasPeriodo, $FechaInicio);
    }
});

$(document).on("change", "input[name='Fecha_Fin']", function () {
    var $FechaFin = $(this).val();
    var $FechaInicio = $("input[name='Fecha_Inicio'").val();
    

    var diferencia = Math.floor((Date.parse($FechaFin) - Date.parse($FechaInicio)) / 86400000)+1;
    if (diferencia < 0) {
        diferencia = diferencia * (-1);
    }
    
    var x = document.getElementById("tiponomina").value;
    setNombrePeriodo(x);

    var $DiasPeriodo = $("input[name='DiasPeriodo'").val(diferencia);
    //if ($DiasPeriodo != "" && $FechaInicio != "") {
    //    calcularDiasPeriodo($DiasPeriodo, $FechaInicio);
    //}
});

function calcularDiasPeriodo($DiasPeriodo, $FechaInicio) {
    var FechaInicio = new Date($FechaInicio);
    FechaInicio.setDate(FechaInicio.getDate() + parseInt($DiasPeriodo));
    var FechaFinal = utils.dateToString(FechaInicio);
    $("input[name='Fecha_Fin']").val(FechaFinal);

    var x = document.getElementById("tiponomina").value;
    setNombrePeriodo(x);

}


function setNombrePeriodo(tipo) {

    var from = $("#Fecha_Inicio").val().split("-");
    var fi = new Date(from[0], from[1] - 1, from[2]);

    from = $("#Fecha_Fin").val().split("-");
    var ff = new Date(from[0], from[1] - 1, from[2]);

    var month = new Array();
    month[0] = "Enero";
    month[1] = "Febrero";
    month[2] = "Marzo";
    month[3] = "Abril";
    month[4] = "Mayo";
    month[5] = "Junio";
    month[6] = "Julio";
    month[7] = "Agosto";
    month[8] = "Septiembre";
    month[9] = "Octubre";
    month[10] = "Noviembre";
    month[11] = "Diciembre";

    var mes = month[ff.getMonth()];

    var ini = fi.getDate();
    var fin = ff.getDate();
    var annio = ff.getFullYear();
    var tipoNombre = " -- ";

    if (tipo == 1) {
        tipoNombre = " DIA ";
    }
    else if (tipo == 2) {
        tipoNombre = " SEM ";
    }
    else if (tipo == 3) {
        tipoNombre = " CAT ";
    }
    else if (tipo == 4) {
        tipoNombre = " QN ";
    }
    else if (tipo == 5) {
        tipoNombre = " MEN ";
    }
    else if (tipo == 11) {
        tipoNombre = " FIN  ";
    }
    else if (tipo == 12) {
        tipoNombre = " AGUINALDO " + annio;;
    }
    else if (tipo == 16) {
        tipoNombre = " ASIM ";
    }

    var descripcion = "";

    if (tipo < 11) {
        descripcion = "Nom" + tipoNombre + ini + " al " + fin + " " + mes + " " + annio;
        $('#Descripcion').val(descripcion);
    } else if (tipo === 11) {
        //descripcion = tipoNombre;
       // alert(tipo);
    } else if (tipo > 11) {
        descripcion = tipoNombre;
        $('#Descripcion').val(descripcion);
    }

 

}



$(document).on("click", ".btnAsignarEmpleadosPeriodo", function () {
    var seleccionados = $("#tblEmpleadosPeriodo").find(".selected");
    
    var array = [];
    var i = 0;
    seleccionados.each(function () {
        array[i] = parseInt($(this).data("idempleado"));
        i++;
    });

    var IdPeriodo = parseInt($("#IdPeriodoPago").val());

    $.ajax({
        url: "/PeriodosPago/AsignarEmpleadosAPeriodo/",
        method: "POST",
        data: {
            IdPeriodoPago: IdPeriodo,
            empleados: array
        },
    }).always(function () {
        utils.loadMainPage("PeriodosPago", "GetPeriodosPago", "#main-content", true);
    });

});

//-------------------------- FIN CREAR PERIODOS DE PAGO --------------------------------------------------------

/* -------------------------------- DETALLE DE PERIODO DE PAGO ---------------------------------------------------- */
$(document).on("shown.bs.modal", "#modal-seleccion-empleados", function () {
    var IdPeriodo = parseInt($("input[name='IdPeriodoPago'").val());
    var aRequest = utils.loadMainPage("PeriodosPago", "SeleccionEmpleadosPeriodo", "#modal-seleccion-empleados-body", true, IdPeriodo);
    aRequest.done(function () {
        $(".modal ").find(".main-content-title").hide();
    });
});

$(document).on("hidden.bs.modal", "#modal-seleccion-empleados", function () {
    $("#tblEmpleadosPeriodo").DataTable().destroy();
    $("#modal-seleccion-empleados-body").html("");
});

$(document).on("click", "#btnEditarPeriodo", function () {
    $(".module.periodoDetalle").find("input").attr("disabled", false);
    $(".module.periodoDetalle").find("select").attr("disabled", false);
    $(this).hide();
    var btnSubmit = $(".module.periodoDetalle").find(".btnSubmit");
    btnSubmit.removeClass("hide");
    btnSubmit.attr("disabled", false);
});

$(document).on("click", "#modal-seleccion-empleados #btnAsignarEmpleados", function () {
    var seleccionados = $("#tblEmpleadosPeriodo").find(".selected");

    var array = [];
    var i = 0;
    seleccionados.each(function () {
        array[i] = parseInt($(this).data("idempleado"));
        i++;
    });

    var IdPeriodo = parseInt($("#IdPeriodoPago").val());

    $.ajax({
        url: "/PeriodosPago/AsignarEmpleadosAPeriodo/",
        method: "POST",
        data: {
            IdPeriodoPago: IdPeriodo,
            empleados: array
        },
    }).always(function () {
        $("#modal-seleccion-empleados").modal("hide");
    });
});

