document.addEventListener("DOMContentLoaded", () => {
    const pageSize = 8;

    const showAppAlert = (message, type = "success") => {
        if (!message) {
            return;
        }

        const alert = document.createElement("div");
        alert.className = `app-alert alert alert-${type} alert-dismissible fade show`;
        alert.setAttribute("role", "alert");
        alert.innerHTML = `
      <i class="bi ${type === "success" ? "bi-check-circle-fill" : "bi-exclamation-triangle-fill"}"></i>
      <span>${message}</span>
      <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

        document.body.appendChild(alert);
        window.setTimeout(() => {
            if (alert.isConnected) {
                alert.remove();
            }
        }, 4200);
    };

    const pendingMessage = sessionStorage.getItem("cms-success-message");
    if (pendingMessage) {
        sessionStorage.removeItem("cms-success-message");
        showAppAlert(pendingMessage);
    }

    document.querySelectorAll("form[data-success-message]").forEach((form) => {
        form.addEventListener("submit", () => {
            sessionStorage.setItem("cms-success-message", form.dataset.successMessage);
        });
    });

    document.querySelectorAll("form[data-confirm]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            if (!window.confirm(form.dataset.confirm)) {
                event.preventDefault();
                sessionStorage.removeItem("cms-success-message");
            }
        });
    });

    document.querySelectorAll("[data-admin-table]").forEach((table) => {
        const card = table.closest(".content-card") || document;
        const searchInput = card.querySelector("[data-table-search]");
        const pagination = card.querySelector("[data-table-pagination]");
        const rows = Array.from(table.querySelectorAll("tbody tr"));
        let currentPage = 1;

        const getFilteredRows = () => {
            const term = (searchInput?.value || "").trim().toLowerCase();
            return rows.filter((row) => row.textContent.toLowerCase().includes(term));
        };

        const render = () => {
            const filteredRows = getFilteredRows();
            const totalPages = Math.max(1, Math.ceil(filteredRows.length / pageSize));
            currentPage = Math.min(currentPage, totalPages);

            rows.forEach((row) => {
                row.hidden = true;
            });

            filteredRows
                .slice((currentPage - 1) * pageSize, currentPage * pageSize)
                .forEach((row) => {
                    row.hidden = false;
                });

            if (!pagination) {
                return;
            }

            pagination.innerHTML = "";

            const summary = document.createElement("span");
            summary.className = "table-summary";
            summary.textContent = `عرض ${filteredRows.length === 0 ? 0 : ((currentPage - 1) * pageSize) + 1}-${Math.min(currentPage * pageSize, filteredRows.length)} من ${filteredRows.length}`;

            const controls = document.createElement("div");
            controls.className = "pagination-controls";

            const prev = document.createElement("button");
            prev.type = "button";
            prev.className = "btn btn-sm btn-outline-secondary";
            prev.textContent = "السابق";
            prev.disabled = currentPage === 1;
            prev.addEventListener("click", () => {
                currentPage -= 1;
                render();
            });

            const page = document.createElement("span");
            page.className = "page-indicator";
            page.textContent = `${currentPage} / ${totalPages}`;

            const next = document.createElement("button");
            next.type = "button";
            next.className = "btn btn-sm btn-outline-secondary";
            next.textContent = "التالي";
            next.disabled = currentPage === totalPages;
            next.addEventListener("click", () => {
                currentPage += 1;
                render();
            });

            controls.append(prev, page, next);
            pagination.append(summary, controls);
        };

        searchInput?.addEventListener("input", () => {
            currentPage = 1;
            render();
        });

        render();
    });

    document.querySelectorAll("[data-visit-calculator]").forEach((form) => {
        const examInput = form.querySelector("[data-exam-price]");
        const proceduresInput = form.querySelector("[data-procedures-price]");
        const manualPriceInput = form.querySelector("[data-manual-price]");
        const discountInput = form.querySelector("[data-discount]");
        const totalInput = form.querySelector("[data-total-price]");
        const paidAmountInput = form.querySelector("[data-paid-amount]");
        const paidCheckbox = form.querySelector("[data-paid-checkbox]");

        const allowDiscount = form.dataset.allowDiscount === "true";
        const maxDiscount = Number.parseFloat(form.dataset.maxDiscount || "0") || 0;

        if (!allowDiscount && discountInput) {
            discountInput.value = "0";
            discountInput.readOnly = true;
        }

        const readNumber = (input) => Number.parseFloat(input?.value || "0") || 0;

        const calculateProcedures = () => {
            let sum = 0;

            form.querySelectorAll(".procedure-row").forEach((row) => {
                const select = row.querySelector("[data-procedure-select]");
                const quantity = readNumber(row.querySelector("[data-procedure-quantity]")) || 1;
                const selectedOption = select?.selectedOptions?.[0];
                const price = Number.parseFloat(selectedOption?.dataset.price || "0") || 0;
                sum += price * quantity;
            });

            const manualPrice = readNumber(manualPriceInput);
            sum += manualPrice;

            if (proceduresInput) {
                proceduresInput.value = sum.toFixed(2);
            }

            return sum;
        };

        const calculateTotal = () => {
            const exam = readNumber(examInput);
            const procedures = calculateProcedures();
            let discount = readNumber(discountInput);

            if (!allowDiscount) {
                discount = 0;
            }
            if (maxDiscount > 0 && discount > maxDiscount) {
                discount = maxDiscount;
                if (discountInput) discountInput.value = discount.toFixed(2);
            }

            const subtotal = exam + procedures;
            const total = Math.max(0, subtotal - discount);

            if (totalInput) {
                totalInput.value = total.toFixed(2);
            }

            if (paidAmountInput && paidCheckbox) {
                const paid = readNumber(paidAmountInput);
                if (paid === total && total > 0) {
                    paidCheckbox.checked = true;
                }
            }
        };

        form.addEventListener("input", calculateTotal);
        calculateTotal();
    });

    const createRevenueGradient = (canvas, color = "rgba(15, 118, 110, 0.24)") => {
        const context = canvas.getContext("2d");
        const gradient = context.createLinearGradient(0, 0, 0, 260);
        gradient.addColorStop(0, color);
        gradient.addColorStop(1, "rgba(15, 118, 110, 0)");
        return gradient;
    };

    const buildLineChart = (canvas, labels, revenue, visits) => {
        const datasets = [
            {
                label: "الإيرادات",
                data: revenue,
                borderColor: "#0f766e",
                backgroundColor: createRevenueGradient(canvas),
                borderWidth: 3,
                fill: true,
                pointBackgroundColor: "#ffffff",
                pointBorderColor: "#0f766e",
                pointBorderWidth: 3,
                pointRadius: 5,
                pointHoverRadius: 7,
                tension: 0.42,
                yAxisID: "income"
            }
        ];

        if (visits) {
            datasets.push({
                label: "الزيارات",
                data: visits,
                borderColor: "#2563eb",
                backgroundColor: "rgba(37, 99, 235, .08)",
                borderWidth: 3,
                fill: false,
                pointBackgroundColor: "#ffffff",
                pointBorderColor: "#2563eb",
                pointBorderWidth: 3,
                pointRadius: 5,
                pointHoverRadius: 7,
                tension: 0.42,
                yAxisID: "visits"
            });
        }

        return new Chart(canvas, {
            type: "line",
            data: { labels, datasets },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                layout: {
                    padding: {
                        top: 25,
                        bottom: 10,
                        left: 20,
                        right: 40
                    }
                },
                interaction: { intersect: false, mode: "index" },
                plugins: {
                    legend: {
                        display: true,
                        labels: {
                            usePointStyle: true,
                            font: { family: "Cairo", weight: "700" },
                            padding: 20
                        }
                    },
                    tooltip: {
                        rtl: true,
                        textDirection: "rtl",
                        callbacks: {
                            label: (context) => context.dataset.yAxisID === "income"
                                ? `${context.parsed.y.toLocaleString("ar-EG")} ج.م`
                                : `${context.parsed.y.toLocaleString("ar-EG")} زيارة`
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: {
                            color: "#6b7280",
                            font: { family: "Cairo", weight: "700" },
                            padding: 10
                        }
                    },
                    income: {
                        beginAtZero: true,
                        position: "right",
                        border: { display: false },
                        grid: { color: "rgba(148, 163, 184, .22)" },
                        grace: '10%',
                        ticks: {
                            color: "#6b7280",
                            callback: (value) => `${Number(value).toLocaleString("ar-EG")} ج.م`,
                            font: { family: "Cairo", weight: "700" },
                            padding: 10
                        }
                    },
                    visits: {
                        beginAtZero: true,
                        position: "left",
                        border: { display: false },
                        grid: { drawOnChartArea: false },
                        grace: '5%',
                        ticks: {
                            color: "#6b7280",
                            precision: 0,
                            font: { family: "Cairo", weight: "700" },
                            padding: 10
                        }
                    }
                }
            }
        });
    };

    const homeRevenueCanvas = document.getElementById("homeRevenueChart");

    if (homeRevenueCanvas && typeof Chart !== "undefined") {
        const chart = buildLineChart(
            homeRevenueCanvas,
            JSON.parse(homeRevenueCanvas.dataset.labels || "[]"),
            JSON.parse(homeRevenueCanvas.dataset.revenue || "[]"),
            JSON.parse(homeRevenueCanvas.dataset.visits || "[]")
        );

        document.querySelector("[data-home-chart-period]")?.addEventListener("change", async (event) => {
            const url = `${homeRevenueCanvas.dataset.url}?period=${encodeURIComponent(event.target.value)}`;
            const response = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
            const data = await response.json();

            chart.data.labels = data.labels;
            chart.data.datasets[0].data = data.revenue;
            chart.data.datasets[1].data = data.visits;
            chart.update();
        });
    }

    ["monthlyAnalyticsChart", "yearlyAnalyticsChart"].forEach((chartId) => {
        const canvas = document.getElementById(chartId);
        if (!canvas || typeof Chart === "undefined") {
            return;
        }

        buildLineChart(
            canvas,
            JSON.parse(canvas.dataset.labels || "[]"),
            JSON.parse(canvas.dataset.revenue || "[]")
        );
    });

    const revenueCanvas = document.getElementById("revenueChart");

    if (revenueCanvas && typeof Chart !== "undefined") {
        const context = revenueCanvas.getContext("2d");
        const chartLabels = JSON.parse(revenueCanvas.dataset.labels || "[]");
        const incomeData = JSON.parse(revenueCanvas.dataset.income || "[]");
        const patientData = JSON.parse(revenueCanvas.dataset.patients || "[]");
        const gradient = context.createLinearGradient(0, 0, 0, 260);
        gradient.addColorStop(0, "rgba(15, 118, 110, 0.24)");
        gradient.addColorStop(1, "rgba(15, 118, 110, 0)");

        new Chart(revenueCanvas, {
            type: "line",
            data: {
                labels: chartLabels,
                datasets: [
                    {
                        label: "الإيرادات",
                        data: incomeData,
                        borderColor: "#0f766e",
                        backgroundColor: gradient,
                        borderWidth: 3,
                        fill: true,
                        pointBackgroundColor: "#ffffff",
                        pointBorderColor: "#0f766e",
                        pointBorderWidth: 3,
                        pointRadius: 5,
                        pointHoverRadius: 7,
                        tension: 0.42,
                        yAxisID: "income"
                    },
                    {
                        label: "المرضى",
                        data: patientData,
                        borderColor: "#2563eb",
                        backgroundColor: "rgba(37, 99, 235, .08)",
                        borderWidth: 3,
                        fill: false,
                        pointBackgroundColor: "#ffffff",
                        pointBorderColor: "#2563eb",
                        pointBorderWidth: 3,
                        pointRadius: 5,
                        pointHoverRadius: 7,
                        tension: 0.42,
                        yAxisID: "patients"
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: "index"
                },
                plugins: {
                    legend: {
                        display: true,
                        labels: {
                            usePointStyle: true,
                            font: {
                                family: "Cairo",
                                weight: "700"
                            }
                        }
                    },
                    tooltip: {
                        rtl: true,
                        textDirection: "rtl",
                        callbacks: {
                            label: (context) => `${context.parsed.y.toLocaleString("ar-EG")} ج.م`
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: "#6b7280",
                            font: {
                                family: "Cairo",
                                weight: "700"
                            }
                        }
                    },
                    income: {
                        beginAtZero: true,
                        position: "right",
                        border: {
                            display: false
                        },
                        grid: {
                            color: "rgba(148, 163, 184, .22)"
                        },
                        ticks: {
                            color: "#6b7280",
                            callback: (value) => `${Number(value).toLocaleString("ar-EG")} ج.م`,
                            font: {
                                family: "Cairo",
                                weight: "700"
                            }
                        }
                    },
                    patients: {
                        beginAtZero: true,
                        position: "left",
                        border: {
                            display: false
                        },
                        grid: {
                            drawOnChartArea: false
                        },
                        ticks: {
                            color: "#6b7280",
                            precision: 0,
                            font: {
                                family: "Cairo",
                                weight: "700"
                            }
                        }
                    }
                }
            }
        });
    }
});

// 🛡️ تنظيم وحماية كود الـ Sidebar Scroll
document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.querySelector('.app-sidebar');
    if (!sidebar) return;

    const scrollPos = localStorage.getItem('sidebar-scroll');
    if (scrollPos) {
        sidebar.scrollTop = scrollPos;
    }

    const sidebarLinks = document.querySelectorAll('.app-sidebar a');
    sidebarLinks.forEach(link => {
        link.addEventListener('click', () => {
            localStorage.setItem('sidebar-scroll', sidebar.scrollTop);
        });
    });
});

// 🛡️ تنظيم وحماية كود الـ Add Procedure Button داخل الـ DOMContentLoaded لمنع الـ Null Errors
document.addEventListener("DOMContentLoaded", function () {
    const addProcedureBtn = document.getElementById('add-procedure-button');
    const container = document.getElementById('procedures-container');

    if (!addProcedureBtn || !container) return;

    addProcedureBtn.addEventListener('click', function () {
        const rows = container.querySelectorAll('.procedure-row');
        const newIndex = rows.length;

        const firstSelect = container.querySelector('[data-procedure-select]');
        if (!firstSelect) return;

        const optionsHtml = firstSelect.innerHTML;

        const newRowHtml = `
            <div class="row g-2 mb-2 procedure-row">
                <input type="hidden" name="Procedures.Index" value="${newIndex}" />
                <div class="col-md-8">
                    <select name="Procedures[${newIndex}].ProcedureId" class="form-select" data-procedure-select>
                        ${optionsHtml}
                    </select>
                </div>
                <div class="col-md-4">
                    <input name="Procedures[${newIndex}].Quantity" value="1" type="number" min="1" class="form-control" data-procedure-quantity />
                </div>
            </div>`;

        container.insertAdjacentHTML('beforeend', newRowHtml);
    });
});

// 🛡️ كود الـ Patient Quick View والـ Eye Button متأمن ومحمي بالكامل
document.addEventListener("DOMContentLoaded", function () {
    const patientSelect = document.querySelector(".select-patient-trigger");
    let loadedVisits = [];

    if (!patientSelect) return;

    patientSelect.addEventListener("change", function () {
        const patientId = this.value;
        const emptyState = document.getElementById("emptySummaryState");
        const content = document.getElementById("summaryContent");
        const visitsList = document.getElementById("patientVisitsList");
        const conditionsDiv = document.getElementById("patientConditions");

        if (!patientId) {
            if (emptyState) emptyState.classList.remove("d-none");
            if (content) content.classList.add("d-none");
            return;
        }

        fetch(`/Visits/GetPatientSummaryJson?patientId=${patientId}`)
            .then(res => res.json())
            .then(data => {
                if (emptyState) emptyState.classList.add("d-none");
                if (content) content.classList.remove("d-none");

                if (conditionsDiv) {
                    conditionsDiv.innerHTML = "";
                    if (data.conditions && data.conditions.length > 0) {
                        data.conditions.forEach(c => {
                            conditionsDiv.innerHTML += `<span class="badge bg-danger-subtle text-danger">${c}</span>`;
                        });
                    } else {
                        conditionsDiv.innerHTML = `<span class="text-muted" style="font-size:0.8rem;">لا توجد أمراض مزمنة مسجلة.</span>`;
                    }
                }

                loadedVisits = data.visits || [];

                if (visitsList) {
                    visitsList.innerHTML = "";
                    if (loadedVisits.length > 0) {
                        loadedVisits.forEach((v, index) => {
                            visitsList.innerHTML += `
                                <div class="p-2 border rounded-2 bg-light d-flex justify-content-between align-items-center" style="font-size:0.85rem;">
                                    <div class="flex-grow-1 text-truncate" style="max-width: 80%;">
                                        <div class="d-flex justify-content-between font-weight-bold text-dark">
                                            <span>د. ${v.doctor}</span>
                                            <small class="text-muted">${v.date}</small>
                                        </div>
                                        <div class="text-secondary mt-1 border-top pt-1 text-truncate" title="${v.notes}">
                                            <strong>ملاحظة:</strong> ${v.notes}
                                        </div>
                                    </div>
                                    <div>
                                        <button type="button" class="btn btn-sm btn-outline-primary rounded-circle px-2 quick-view-btn" data-index="${index}" title="عرض خاطف للزيارة">
                                            <i class="bi bi-eye-fill"></i>
                                        </button>
                                    </div>
                                </div>
                            `;
                        });

                        setupQuickViewButtons();
                    } else {
                        visitsList.innerHTML = `<div class="text-center text-muted py-3" style="font-size:0.8rem;">زيارة أولى للمريض.</div>`;
                    }
                }
            })
            .catch(err => console.error("Error fetching safety details:", err));
    });

    function setupQuickViewButtons() {
        const buttons = document.querySelectorAll(".quick-view-btn");
        const modalBody = document.getElementById("quickViewModalBody");
        const modalElement = document.getElementById('visitQuickViewModal');

        if (!modalElement || !modalBody) return;

        buttons.forEach(btn => {
            btn.addEventListener("click", function () {
                const index = this.getAttribute("data-index");
                const visit = loadedVisits[index];

                if (visit) {
                    modalBody.innerHTML = `
                        <div class="mb-3">
                            <label class="text-muted small d-block">الطبيب المعالج</label>
                            <div class="fw-bold text-dark" style="font-size:1.1rem;"><i class="bi bi-person-badge"></i> د. ${visit.doctor}</div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-6">
                                <label class="text-muted small d-block">تاريخ الزيارة</label>
                                <div class="fw-bold text-secondary"><i class="bi bi-calendar-event"></i> ${visit.date}</div>
                            </div>
                            <div class="col-6">
                                <label class="text-muted small d-block">التكلفة الإجمالية</label>
                                <div class="fw-bold text-success"><i class="bi bi-cash-coin"></i> ${visit.price} ج.م</div>
                            </div>
                        </div>
                        <hr/>
                        <div class="mb-3">
                            <label class="text-muted small d-block fw-bold text-primary">التشخيص والملاحظات الطبية:</label>
                            <div class="p-3 bg-light rounded-3 mt-1 border-start border-3 border-primary" style="white-space: pre-line; line-height:1.5;">
                                ${visit.notes || "لا توجد ملاحظات مسجلة."}
                            </div>
                        </div>
                        <div class="d-grid gap-2 mt-4">
                            <a href="/Visits/Details/${visit.id}" class="btn btn-primary d-flex align-content-center justify-content-center gap-2 py-2 fw-bold shadow-sm">
                                <i class="bi bi-arrow-left-right"></i>
                                عرض تفاصيل الزيارة كاملة
                            </a>
                        </div>
                    `;
                    const myModal = new bootstrap.Modal(modalElement);
                    myModal.show();
                }
            });
        });
    }
});