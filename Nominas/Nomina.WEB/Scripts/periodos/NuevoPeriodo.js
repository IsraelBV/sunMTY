function comprobarSiBisisesto(anio) {
    if ((anio % 100 != 0) && ((anio % 4 == 0) || (anio % 400 == 0))) {
        return true;
    }
    else {
        return false;
    }
}


$(".btnEmpleadosNormal").click(function () {
    $("#activos").removeClass("btn-desactivado");
    $("#baja").removeClass("btn-activo");
    $("#rfc").removeClass("btn-activo");
    $("#activos").addClass("btn-activo");
    $("#baja").addClass("btn-desactivado");
    $("#rfc").addClass("btn-desactivado");

    var x = document.getElementById("tiponomina").value;
    if (x != 0) {
        if (x == 11) {

            $("#empleado").load("/PeriodosPago/empleadoFiniquito/?idPeriocidadPago=" + x + "&rfc=1");

        } else {
            $("#empleado").load("/PeriodosPago/GetEmpleadoByTipoNomina/?idPeriocidadPago=" + x + "&rfc=1");
        }

    }
});

$(".btnEmpleadoRFCNV").click(function () {
    $("#activos").removeClass("btn-activo");
    $("#baja").removeClass("btn-activo");
    $("#rfc").removeClass("btn-desactivado");
    $("#activos").addClass("btn-desactivado");
    $("#baja").addClass("btn-desactivado");
    $("#rfc").addClass("btn-activo");
    var x = document.getElementById("tiponomina").value;
    if (x != 0) {
        if (x == 11) {
            $("#empleado").load("/PeriodosPago/empleadoFiniquito/?idPeriocidadPago=" + x + "&rfc=0");

        } else {
            $("#empleado").load("/PeriodosPago/GetEmpleadoByTipoNomina/?idPeriocidadPago=" + x + "&rfc=0");
        }

    }
});

$(".btnEmpleadoBaja").click(function () {
    $("#activos").removeClass("btn-activo");
    $("#baja").removeClass("btn-desactivado");
    $("#rfc").removeClass("btn-activo");
    $("#activos").addClass("btn-desactivado");
    $("#baja").addClass("btn-activo");
    $("#rfc").addClass("btn-desactivado");

    var x = document.getElementById("tiponomina").value;
    if (x != 0) {
        $("#empleado").load("/PeriodosPago/EmpleadosBaja/?idPeriocidadPago=" + x);
    }
});