$(document).ready(function () {

    var tableperi = $('#tablaPeriodos').DataTable({
        dom: 'Bfrtip',
        "paging": false,
        "scrollY": "75vh",
        "select": true,
        "language": { "url": "../scripts/datatables-spanish.json" },
        "order": [[0, "desc"]],
        "buttons": [
        {
            className: '',
            text: 'Detalle Periodo',
            action: function () {
                var $row = $("#tablaPeriodos").find(".selected");
                if ($row.length > 0) {
                    var IdPeriodo = $row.data("idperiodopago");
                    var modalDetalle = $("#modal-Detalleperiodo");
                    $.get("../../PeriodosPago/PeriodoDetalle?id=" + IdPeriodo, function (data) {
                        $("#modalDetallePeriodo").html(data);
                        modalDetalle.modal();
                    });
                }
            },
            key: {
                key: 'd',

            }
        },
        {
            className: 'crearPeriodo',
            text: 'Crear Periodo',
            action: function () {
                var modalDetalle = $("#modal-periodo");
                $.get("../../PeriodosPago/NuevoPeriodo", function (data) {
                    $("#modalPeriodo").html(data);
                    modalDetalle.modal();
                });
            }
        },
         {
             className: 'eliminarPeriodo',
             text: 'Eliminar Periodo',
             action: function () {
                 var $row = $("#tablaPeriodos").find(".selected");

                 var IdPeriodo = $row.data("idperiodopago");

                 if ($.isNumeric(IdPeriodo)) {
                     utils.confirmDialog("¿DESEA CONTINUAR?", "Se Eliminrá el Periodo: " + IdPeriodo, "CONFIRMAR", "CANCELAR", function (response) {
                         if (response) {
                             var request = $.ajax({
                                 url: "/PeriodosPago/eliminarPeriodo/",
                                 method: "POST",
                                 data: {

                                     idPeriodo: IdPeriodo
                                 }
                             });
                             request.done(function (data) {
                                 console.log(data);
                                 console.log(data.length);
                                 console.log(data[0]);

                                 utils.showMessage("Eliminar periodos", data, 5000, "");

                                 $("#main-content").load("/PeriodosPago/GetPeriodosPago/");

                             });
                         }
                     });
                 }
             }
         }

        ],
    });

    $('#tablaPeriodos tbody').on('dblclick', 'tr', function () {
        var datarow = tableperi.row(this).data();

        var modalDetalle = $("#modal-Detalleperiodo");
        $.get("../../PeriodosPago/PeriodoDetalle?id=" + datarow[0], function (data) {
            $("#modalDetallePeriodo").html(data);
            modalDetalle.modal();
        });
    });

    $("#modal-periodo").on("shown.bs.modal", function () {
        var table = $('#normal').DataTable({
            "paging": false,
            "scrollY": "200px",
        });
    });

    $("#modal-Detalleperiodo").on("shown.bs.modal", function () {
        var tableDetalle = $('#empleadosPeri').DataTable({
            dom: 'Bfrtip',
            "language": {
                "url": "../scripts/datatables-spanish.json",
                "select": {
                    "rows": {
                        _: "%d registros seleccionados",
                        0: "Ningun Registro Seleccionado",
                        1: "%d registro seleccionado"
                    }
                }

            },

            "paging": false,
            "scrollY": "200px",
            "select": {
                style: 'multi'
            }, "buttons": [
                  {
                      className: 'seleccionarall',
                      text: 'Seleccionar: Todos',
                      action: function () {
                          tableDetalle.rows().select();
                      }, key: {

                          key: 'a',

                      }
                  },
              {
                  className: 'seleccionarcero',
                  text: 'Seleccionar: Ninguno',
                  action: function () {
                      tableDetalle.rows().deselect();
                  },
                  key: {
                      key: 'd',

                  }
              },
            {
                className: 'borrarRegistro',
                text: 'Eliminar',
                action: function () {
                    arrayE = [];
                    var selected = $("#empleadosPeri").find(".selected");
                    selected.each(function () {
                        var id = parseInt($(this).data("idempleado"));
                        arrayE.push(id);
                    });
                    var IdPeriodo = selected.data("idperiodopago");

                    if (arrayE.length > 0) {
                        var request = $.ajax({
                            url: "/PeriodosPago/eliminarLista/",
                            method: "POST",
                            data: {
                                arrayE: arrayE,
                                idPeriodo: IdPeriodo
                            }
                        });
                        request.done(function (data) {
                            $("#empleadosDetalles").load("/PeriodosPago/empleadosAgregados/?idPeriodo=" + IdPeriodo);
                        });
                    }

                },
                key: {
                    key: 'd'

                }
            }]
        });
    });

    //si el modal se cierra se actualiza el main-content
    $('#modal-Detalleperiodo').on('hidden.bs.modal',
        function () {
            $("#main-content").load("/PeriodosPago/GetPeriodosPago/");
        });

});