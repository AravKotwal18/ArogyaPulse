// ==========================================================================
// ArogyaPulse Frontend Engine v2.0
// AI-Powered Rural Healthcare Triage Architecture
// ==========================================================================

const API_BASE = '/api';

// Global Application State
const state = {
    currentPage: 'home',
    language: 'en', // 'en' or 'hi'
    patients: [],
    villageStats: [],
    systemStats: null,
    queueFilter: {
        risk: 'all',
        village: 'all',
        search: ''
    },
    chatMessages: [],
    healthChartInstance: null,
    villageChartInstance: null,
    loading: false
};

// Bilingual Dictionaries
const i18n = {
    en: {
        overview: 'Overview',
        intake: 'ASHA Intake',
        doctor: 'Doctor Dashboard',
        bot: 'ASHA Bot AI',
        villages: 'Village Clusters',
        engine: 'Triage Engine',
        heroTitle: '🏥 AI-Powered Rural Healthcare Triage',
        heroSub: 'Bridging the critical gap between frontline ASHA community workers and remote district physicians with instant WHO vitals analysis, automated risk stratification, and bilingual clinical guidance.',
        startIntake: '📋 Start Patient Intake',
        viewDashboard: '👨‍⚕️ View Doctor Dashboard',
        screenedPatients: 'Screened Patients',
        syncAccuracy: 'Sync Accuracy',
        avgReferralTime: 'Avg Referral Time',
        connectedVillages: 'Connected Villages',
        highRiskAlertsSent: 'High Risk Alerts Sent',
        howItWorksTitle: '⚡ How ArogyaPulse Triage Operates',
        howItWorksSub: 'A seamless 3-step pipeline engineered for low-connectivity rural health posts.',
        step1Title: '1️⃣ ASHA Intake & Live Vitals',
        step1Text: 'Frontline ASHA workers record patient vitals (BP, SpO2, temp, glucose). Live clinical algorithms calculate instant risk preview on device.',
        step2Title: '2️⃣ Automated Risk Stratification',
        step2Text: 'Deterministic clinical triage engine categorizes cases into High Risk (Immediate Referral), Medium Risk (24-48h Review), or Low Risk.',
        step3Title: '3️⃣ Doctor Review & Action',
        step3Text: 'District physicians access a real-time color-coded queue, review diagnostic breakdowns, update patient status, and dispatch WhatsApp emergency alerts.',
        
        // Form & Intake
        intakeHeader: '📋 ASHA Patient Registration & Vitals Intake',
        patientNameLabel: 'Patient Full Name *',
        ageLabel: 'Age (Years) *',
        genderLabel: 'Gender *',
        bloodGroupLabel: 'Blood Group',
        villageLabel: 'Village Cluster *',
        isPregnantLabel: 'Obstetric Status: Is Pregnant?',
        vitalSignsHeader: '🩺 Physiological Vital Signs',
        bpLabel: 'Blood Pressure (Systolic/Diastolic mmHg) *',
        spo2Label: 'Blood Oxygen Saturation (SpO2 %) *',
        tempLabel: 'Body Temperature (°C) *',
        glucoseLabel: 'Blood Glucose (mg/dL) *',
        symptomsLabel: 'Observed Symptoms & Medical History',
        submitIntakeBtn: '🚀 Calculate Triage & Register Patient',
        liveCalcTitle: '⚡ Real-Time Clinical Triage Preview',
        liveCalcPrompt: 'Enter vitals above to see immediate risk score calculation.',

        // Doctor Dashboard
        doctorDashboardHeader: '👨‍⚕️ District Physician Triage Dashboard',
        doctorDashboardSub: 'Real-time prioritized triage queue sorted by WHO severity scores.',
        queueStatsHigh: 'High Risk (Immediate)',
        queueStatsMed: 'Medium Risk (24-48h)',
        queueStatsLow: 'Low Risk (Routine)',
        filterAll: 'All Patients',
        filterHigh: 'High Risk',
        filterMedium: 'Medium Risk',
        filterLow: 'Low Risk',
        searchPlaceholder: 'Search by patient name, village, or symptoms...',
        allVillages: 'All Villages',

        // Bot AI
        botHeader: '🤖 ASHA Bot AI Clinical Assistant',
        botSub: 'Interactive bilingual support following WHO & NHM India clinical triage protocols.',
        chatPlaceholder: 'Ask a clinical question (e.g. "What to do if SpO2 < 90%?")...',
        sendBtn: 'Send',
        chipHypoxia: '🫁 Severe Hypoxia (<90%)',
        chipPreeclampsia: '🤰 Pre-Eclampsia Warnings',
        chipBP: '🩸 Hypertensive Crisis Norms',
        chipHindiBP: '🇮🇳 गर्भावस्था में बीपी',

        // Villages
        villagesHeader: '🗺️ Rural Village Cluster Analytics',
        villagesSub: 'Real-time screening coverage and high-risk case distribution across connected health centers.',
        colVillage: 'Village Cluster',
        colScreened: 'Total Screened',
        colHighRisk: 'High Risk Cases',
        colRatio: 'High Risk Ratio',
        colPHC: 'Primary Health Center',
        colAction: 'Action',
        filterQueueBtn: 'View Village Patients',

        // Triage Engine Simulator
        engineHeader: '⚙️ ArogyaPulse Clinical Scoring Engine Standards',
        engineSub: 'Deterministic scoring rules based on WHO Primary Health Care & NHM India Guidelines.',
        simTitle: '🧪 Interactive Triage Scoring Simulator',
        simSub: 'Adjust vitals below to test the exact WHO point allocation formula in real-time.'
    },
    hi: {
        overview: 'अवलोकन',
        intake: 'आशा इनटेक',
        doctor: 'डॉक्टर डैशबोर्ड',
        bot: 'आशा बॉट एआई',
        villages: 'गांव क्लस्टर',
        engine: 'ट्राइएज इंजन',
        heroTitle: '🏥 एआई-संचालित ग्रामीण स्वास्थ्य ट्राइएज प्रणाली',
        heroSub: 'फ्रंटलाइन आशा सामुदायिक कार्यकर्ताओं और दूरस्थ जिला डॉक्टरों के बीच अंतर को पाटना। तत्काल विटल्स विश्लेषण और स्वचालित जोखिम वर्गीकरण।',
        startIntake: '📋 नया मरीज दर्ज करें',
        viewDashboard: '👨‍⚕️ डॉक्टर डैशबोर्ड देखें',
        screenedPatients: 'स्क्रीन किए गए मरीज',
        syncAccuracy: 'सिंक सटीकता',
        avgReferralTime: 'औसत रेफरल समय',
        connectedVillages: 'जुड़े हुए गाँव',
        highRiskAlertsSent: 'भेजे गए उच्च जोखिम अलर्ट',
        howItWorksTitle: '⚡ आरोग्यपल्स कैसे काम करता है',
        howItWorksSub: 'कम कनेक्टिविटी वाले ग्रामीण केंद्रों के लिए बनाया गया 3-चरणीय मॉडल।',
        step1Title: '1️⃣ आशा कार्यकर्ता इनटेक',
        step1Text: 'आशा कार्यकर्ता मरीज की जनसांख्यिकी और विटल्स दर्ज करते हैं। डिवाइस पर तुरंत जोखिम स्कोर दिखता है।',
        step2Title: '2️⃣ स्वचालित जोखिम विभाजन',
        step2Text: 'इंजन तुरंत मरीज को उच्च (Emergency), मध्यम (24-48h) या निम्न (Routine) श्रेणी में विभाजित करता है।',
        step3Title: '3️⃣ डॉक्टर समीक्षा और कार्रवाई',
        step3Text: 'जिला डॉक्टर रंग-कोडित ट्राइएज कतार देखते हैं, स्थिति अपडेट करते हैं और व्हाट्सएप अलर्ट भेजते हैं।',

        // Form & Intake
        intakeHeader: '📋 आशा रोगी पंजीकरण एवं विटल्स इनटेक',
        patientNameLabel: 'रोगी का पूरा नाम *',
        ageLabel: 'आयु (वर्ष) *',
        genderLabel: 'लिंग *',
        bloodGroupLabel: 'रक्त समूह',
        villageLabel: 'गाँव का नाम *',
        isPregnantLabel: 'गर्भावस्था स्थिति: क्या गर्भवती है?',
        vitalSignsHeader: '🩺 शारीरिक महत्वपूर्ण संकेत (Vitals)',
        bpLabel: 'रक्तचाप (सिस्टोलिक/डायस्टोलिक mmHg) *',
        spo2Label: 'रक्त ऑक्सीजन (SpO2 %) *',
        tempLabel: 'शरीर का तापमान (°C) *',
        glucoseLabel: 'ब्लड ग्लूकोज (mg/dL) *',
        symptomsLabel: 'लक्षण एवं चिकित्सीय विवरण',
        submitIntakeBtn: '🚀 ट्राइएज स्कोर निकालें और जमा करें',
        liveCalcTitle: '⚡ लाइव क्लिनिकल ट्राइएज पूर्वावलोकन',
        liveCalcPrompt: 'तत्काल स्कोर देखने के लिए ऊपर विटल्स भरें।',

        // Doctor Dashboard
        doctorDashboardHeader: '👨‍⚕️ जिला चिकित्सक ट्राइएज डैशबोर्ड',
        doctorDashboardSub: 'गंभीरता के आधार पर स्वचालित रूप से क्रमित वास्तविक समय कतार।',
        queueStatsHigh: 'उच्च जोखिम (तत्काल)',
        queueStatsMed: 'मध्यम जोखिम (24-48h)',
        queueStatsLow: 'निम्न जोखिम (सामान्य)',
        filterAll: 'सभी मरीज',
        filterHigh: 'उच्च जोखिम',
        filterMedium: 'मध्यम जोखिम',
        filterLow: 'निम्न जोखिम',
        searchPlaceholder: 'मरीज के नाम, गांव या लक्षण से खोजें...',
        allVillages: 'सभी गाँव',

        // Bot AI
        botHeader: '🤖 आशा बॉट एआई क्लिनिकल सहायक',
        botSub: 'WHO और NHM भारत दिशानिर्देशों पर आधारित द्विभाषी सहायता।',
        chatPlaceholder: 'चिकित्सीय प्रश्न पूछें (उदा. "जब SpO2 < 90% हो तो क्या करें?")...',
        sendBtn: 'भेजें',
        chipHypoxia: '🫁 गंभीर हाइपोक्सिया (<90%)',
        chipPreeclampsia: '🤰 प्री-एकलम्पसिया लक्षण',
        chipBP: '🩸 बीपी क्राइसिस सीमाएँ',
        chipHindiBP: '🇮🇳 गर्भावस्था में बीपी',

        // Villages
        villagesHeader: '🗺️ ग्रामीण गांव क्लस्टर विश्लेषण',
        villagesSub: 'विभिन्न स्वास्थ्य केंद्रों पर मरीजों की स्क्रीनिंग और उच्च जोखिम आंकड़े।',
        colVillage: 'गांव का नाम',
        colScreened: 'कुल स्क्रीन किए गए',
        colHighRisk: 'उच्च जोखिम मरीज',
        colRatio: 'उच्च जोखिम अनुपात',
        colPHC: 'प्राथमिक स्वास्थ्य केंद्र',
        colAction: 'कार्रवाई',
        filterQueueBtn: 'इस गांव के मरीज देखें',

        // Triage Engine Simulator
        engineHeader: '⚙️ आरोग्यपल्स क्लिनिकल स्कोरिंग मानक',
        engineSub: 'WHO और NHM भारत दिशानिर्देशों पर आधारित स्कोरिंग नियम।',
        simTitle: '🧪 इंटरैक्टिव ट्राइएज स्कोरिंग सिमुलेटर',
        simSub: 'लाइव अंक गणना देखने के लिए नीचे विटल्स बदलें।'
    }
};

function t(key) {
    return i18n[state.language][key] || key;
}

// Initialization
document.addEventListener('DOMContentLoaded', () => {
    setupGlobalEvents();
    loadSystemStats();
    loadPatients();
    loadVillageStats();
    renderPage('home');
});

function setupGlobalEvents() {
    document.querySelectorAll('.nav-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const page = e.currentTarget.dataset.page;
            if (page) renderPage(page);
        });
    });

    updateOfflineBadge();
    window.addEventListener('online', () => {
        showToast('🟢 Internet connection restored! Auto-syncing offline queue...', 'success');
        syncOfflineQueue();
    });
    window.addEventListener('offline', () => {
        showToast('🔴 Internet disconnected — Switching to local offline mode.', 'warning');
    });
}

function toggleLanguage() {
    state.language = state.language === 'en' ? 'hi' : 'en';
    const langBtn = document.getElementById('langToggleBtn');
    if (langBtn) {
        langBtn.innerHTML = state.language === 'en' 
            ? `<span class="lang-flag">🇮🇳</span><span class="lang-text">हिंदी (HI)</span>`
            : `<span class="lang-flag">🇬🇧</span><span class="lang-text">English (EN)</span>`;
    }
    updateNavLabels();
    renderPage(state.currentPage);
}

function updateNavLabels() {
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.dataset.i18n;
        if (key && i18n[state.language][key]) {
            el.innerText = i18n[state.language][key];
        }
    });
}

// Router & Page Rendering
function renderPage(page) {
    state.currentPage = page;
    const main = document.querySelector('.app-main');

    document.querySelectorAll('.nav-btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.page === page) btn.classList.add('active');
    });

    switch (page) {
        case 'home':
            main.innerHTML = renderHome();
            break;
        case 'intake':
            main.innerHTML = renderIntake();
            setupIntakeListeners();
            break;
        case 'doctor':
            main.innerHTML = renderDoctor();
            loadTriageQueue();
            break;
        case 'bot':
            main.innerHTML = renderBot();
            setupBotChat();
            break;
        case 'villages':
            main.innerHTML = renderVillages();
            break;
        case 'algorithm':
            main.innerHTML = renderAlgorithm();
            setupSimulatorListeners();
            break;
        default:
            main.innerHTML = renderHome();
    }
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ==========================================================================
// 1. Overview (Home) Module
// ==========================================================================
function renderHome() {
    const stats = state.systemStats || {
        totalPatients: 1422,
        syncAccuracy: '99.4%',
        avgReferralTime: '18 min',
        connectedVillages: 4,
        highRiskAlertsSent: 3
    };

    return `
    <div class="hero-banner">
        <h1 class="hero-title">${t('heroTitle')}</h1>
        <p class="hero-sub">${t('heroSub')}</p>
        <div class="hero-stats-grid">
            <div class="stat-card">
                <div class="stat-number">${stats.totalPatients.toLocaleString()}</div>
                <div class="stat-label">${t('screenedPatients')}</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${stats.syncAccuracy}</div>
                <div class="stat-label">${t('syncAccuracy')}</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${stats.avgReferralTime}</div>
                <div class="stat-label">${t('avgReferralTime')}</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${stats.connectedVillages}</div>
                <div class="stat-label">${t('connectedVillages')}</div>
            </div>
        </div>
        <div style="margin-top: 2.5rem; display: flex; gap: 1rem; justify-content: center; flex-wrap: wrap;">
            <button class="btn btn-primary" onclick="renderPage('intake')">${t('startIntake')}</button>
            <button class="btn btn-secondary" onclick="renderPage('doctor')">${t('viewDashboard')}</button>
        </div>
    </div>

    <div class="card">
        <div class="card-header-title">
            <span>${t('howItWorksTitle')}</span>
            <span style="font-size: 0.9rem; color: var(--primary); font-weight: 600;">WHO/NHM Standard</span>
        </div>
        <div class="card-subtitle">${t('howItWorksSub')}</div>
        
        <div class="grid grid-3" style="margin-top: 1.5rem;">
            <div class="card" style="margin-bottom: 0; background: var(--bg-app); border-left: 4px solid var(--primary);">
                <h3 style="margin-bottom: 0.5rem;">${t('step1Title')}</h3>
                <p style="font-size: 0.92rem; color: var(--text-muted);">${t('step1Text')}</p>
            </div>
            <div class="card" style="margin-bottom: 0; background: var(--bg-app); border-left: 4px solid var(--warning);">
                <h3 style="margin-bottom: 0.5rem;">${t('step2Title')}</h3>
                <p style="font-size: 0.92rem; color: var(--text-muted);">${t('step2Text')}</p>
            </div>
            <div class="card" style="margin-bottom: 0; background: var(--bg-app); border-left: 4px solid var(--danger);">
                <h3 style="margin-bottom: 0.5rem;">${t('step3Title')}</h3>
                <p style="font-size: 0.92rem; color: var(--text-muted);">${t('step3Text')}</p>
            </div>
        </div>
    </div>
    `;
}

// ==========================================================================
// 2. ASHA Intake Module & Live Vitals Calculator
// ==========================================================================
function renderIntake() {
    return `
    <div class="card">
        <div class="card-header-title">${t('intakeHeader')}</div>
        <div class="card-subtitle">Record patient demographics and physiological vital signs for instant risk assessment.</div>

        <form id="intakeForm">
            <div class="grid grid-3">
                <div class="form-group">
                    <label class="form-label">${t('patientNameLabel')}</label>
                    <input type="text" class="form-input" id="patientName" placeholder="e.g. Radhika Devi" required>
                </div>
                <div class="form-group">
                    <label class="form-label">${t('ageLabel')}</label>
                    <input type="number" class="form-input" id="patientAge" min="0" max="120" placeholder="e.g. 28" required>
                </div>
                <div class="form-group">
                    <label class="form-label">${t('genderLabel')}</label>
                    <select class="form-select" id="patientGender" required>
                        <option value="Female">Female ♀</option>
                        <option value="Male">Male ♂</option>
                        <option value="Other">Other</option>
                    </select>
                </div>
            </div>

            <div class="grid grid-2" style="margin-top: 0.5rem;">
                <div class="form-group">
                    <label class="form-label">${t('bloodGroupLabel')}</label>
                    <select class="form-select" id="patientBloodGroup">
                        <option value="Unknown">Unknown / Not Tested</option>
                        <option value="A+">A+</option>
                        <option value="A-">A-</option>
                        <option value="B+">B+</option>
                        <option value="B-">B-</option>
                        <option value="O+">O+</option>
                        <option value="O-">O-</option>
                        <option value="AB+">AB+</option>
                        <option value="AB-">AB-</option>
                    </select>
                </div>
                <div class="form-group">
                    <label class="form-label">${t('villageLabel')}</label>
                    <select class="form-select" id="patientVillage" required>
                        <option value="">Select Village...</option>
                        <option value="Nandpur">Nandpur</option>
                        <option value="Laxmipur">Laxmipur</option>
                        <option value="Rampur">Rampur</option>
                        <option value="Devpur">Devpur</option>
                    </select>
                </div>
            </div>

            <div class="form-group" id="pregnancyGroupContainer" style="margin-top: 0.5rem;">
                <label class="form-label" style="display: flex; align-items: center; gap: 0.5rem; cursor: pointer;">
                    <input type="checkbox" id="isPregnant" style="width: 18px; height: 18px; accent-color: var(--primary);">
                    <span>${t('isPregnantLabel')}</span>
                </label>
            </div>

            <hr style="margin: 1.5rem 0; border: none; border-top: 1px solid var(--border-color);">

            <h3 style="margin-bottom: 1rem; font-size: 1.15rem; color: var(--primary-dark);">${t('vitalSignsHeader')}</h3>
            <div class="grid grid-4">
                <div class="form-group">
                    <label class="form-label">${t('bpLabel')}</label>
                    <input type="text" class="form-input vital-calc-trigger" id="bpInput" placeholder="e.g. 140/92" required>
                </div>
                <div class="form-group">
                    <label class="form-label">${t('spo2Label')}</label>
                    <input type="number" class="form-input vital-calc-trigger" id="spO2Input" min="50" max="100" placeholder="e.g. 94" required>
                </div>
                <div class="form-group">
                    <label class="form-label">${t('tempLabel')}</label>
                    <input type="number" step="0.1" class="form-input vital-calc-trigger" id="tempInput" min="30" max="45" placeholder="e.g. 37.5" required>
                </div>
                <div class="form-group">
                    <label class="form-label">${t('glucoseLabel')}</label>
                    <input type="number" class="form-input vital-calc-trigger" id="glucoseInput" min="30" max="500" placeholder="e.g. 110" required>
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">${t('symptomsLabel')}</label>
                <textarea class="form-textarea" id="symptomsInput" placeholder="Describe symptoms (headache, fever, dizziness, swelling)..."></textarea>
            </div>

            <!-- Live Calculator Preview Box -->
            <div class="live-calculator-box" id="liveCalcBox">
                <div class="calc-header">
                    <div class="calc-title">${t('liveCalcTitle')}</div>
                    <span class="calc-score-badge badge badge-low" id="liveScoreBadge">0 PTS • LOW RISK</span>
                </div>
                <div id="liveCalcBreakdown" style="font-size: 0.9rem; color: var(--text-muted);">
                    ${t('liveCalcPrompt')}
                </div>
            </div>

            <div style="margin-top: 1.5rem; text-align: right;">
                <button type="submit" class="btn btn-primary" style="padding: 0.85rem 2rem; font-size: 1rem;">
                    ${t('submitIntakeBtn')}
                </button>
            </div>
        </form>
    </div>
    `;
}

function setupIntakeListeners() {
    const form = document.getElementById('intakeForm');
    if (!form) return;

    const genderSelect = document.getElementById('patientGender');
    if (genderSelect) {
        genderSelect.addEventListener('change', handleGenderChange);
    }

    // Attach live calculator triggers
    document.querySelectorAll('.vital-calc-trigger, #isPregnant').forEach(input => {
        input.addEventListener('input', updateLiveCalculator);
        input.addEventListener('change', updateLiveCalculator);
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        await submitIntake();
    });

    handleGenderChange();
}

function handleGenderChange() {
    const genderSelect = document.getElementById('patientGender');
    const pregContainer = document.getElementById('pregnancyGroupContainer');
    const isPregnant = document.getElementById('isPregnant');

    if (!genderSelect || !pregContainer) return;

    if (genderSelect.value === 'Female') {
        pregContainer.style.display = 'block';
    } else {
        pregContainer.style.display = 'none';
        if (isPregnant) isPregnant.checked = false;
    }
    updateLiveCalculator();
}

function updateLiveCalculator() {
    const bp = document.getElementById('bpInput')?.value || '';
    const spO2 = parseInt(document.getElementById('spO2Input')?.value) || 98;
    const temp = parseFloat(document.getElementById('tempInput')?.value) || 37.0;
    const glucose = parseInt(document.getElementById('glucoseInput')?.value) || 100;
    const gender = document.getElementById('patientGender')?.value || 'Female';
    const isPregnant = (gender === 'Female') && (document.getElementById('isPregnant')?.checked || false);

    let score = 0;
    let breakdown = [];

    // BP Calculation
    if (bp.includes('/')) {
        const parts = bp.split('/');
        const sys = parseInt(parts[0]);
        const dia = parseInt(parts[1]);
        if (!isNaN(sys) && !isNaN(dia)) {
            if (sys >= 160 || dia >= 100) {
                score += 40;
                breakdown.push(`🔴 Hypertensive Crisis (${bp}): +40 pts`);
            } else if (sys >= 140 || dia >= 90) {
                score += 20;
                breakdown.push(`🟠 Stage 1 Hypertension (${bp}): +20 pts`);
            }

            if (isPregnant && (sys >= 140 || dia >= 90)) {
                score += 35;
                breakdown.push(`🤰 Gestational Pre-Eclampsia Risk (Pregnant BP ${bp}): +35 pts`);
            }
        }
    }

    // SpO2
    if (spO2 < 90) {
        score += 35;
        breakdown.push(`🔴 Severe Hypoxia (SpO2 ${spO2}% < 90%): +35 pts`);
    } else if (spO2 <= 94) {
        score += 15;
        breakdown.push(`🟠 Moderate Hypoxemia (SpO2 ${spO2}%): +15 pts`);
    }

    // Glucose
    if (glucose >= 200 || glucose < 70) {
        score += 30;
        breakdown.push(`🟠 Glucose Anomaly (${glucose} mg/dL): +30 pts`);
    }

    // Temp
    if (temp >= 39.0 || temp < 35.0) {
        score += 20;
        breakdown.push(`🟠 Temperature Anomaly (${temp}°C): +20 pts`);
    }

    let riskLevel = 'Low';
    let badgeClass = 'badge-low';
    if (score >= 45) { riskLevel = 'High'; badgeClass = 'badge-high'; }
    else if (score >= 20) { riskLevel = 'Medium'; badgeClass = 'badge-medium'; }

    const badgeEl = document.getElementById('liveScoreBadge');
    const breakdownEl = document.getElementById('liveCalcBreakdown');

    if (badgeEl) {
        badgeEl.className = `calc-score-badge badge ${badgeClass}`;
        badgeEl.innerText = `${score} PTS • ${riskLevel.toUpperCase()} RISK`;
    }

    if (breakdownEl) {
        if (breakdown.length === 0) {
            breakdownEl.innerHTML = `<span style="color: var(--success); font-weight: 600;">✓ All vital signs within normal WHO physiological ranges.</span>`;
        } else {
            breakdownEl.innerHTML = `<ul style="padding-left: 1.25rem;">${breakdown.map(item => `<li>${item}</li>`).join('')}</ul>`;
        }
    }
}

async function submitIntake() {
    const name = document.getElementById('patientName').value.trim();
    const age = parseInt(document.getElementById('patientAge').value);
    const gender = document.getElementById('patientGender').value;
    const bloodGroup = document.getElementById('patientBloodGroup').value;
    const village = document.getElementById('patientVillage').value;
    const isPregnant = (gender === 'Female') && document.getElementById('isPregnant').checked;
    const bp = document.getElementById('bpInput').value.trim();
    const spO2 = parseInt(document.getElementById('spO2Input').value);
    const temp = parseFloat(document.getElementById('tempInput').value);
    const glucose = parseInt(document.getElementById('glucoseInput').value);
    const symptoms = document.getElementById('symptomsInput').value.trim();

    const localRecordId = (typeof crypto !== 'undefined' && crypto.randomUUID) ? crypto.randomUUID() : 'offline-' + Date.now() + '-' + Math.random().toString(36).substring(2, 9);

    const payload = {
        name,
        age,
        gender,
        bloodGroup,
        village,
        isPregnant,
        vitals: { bp, spO2, temp, glucose },
        symptoms,
        localRecordId
    };

    if (!navigator.onLine) {
        saveOfflineRecord(payload);
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/patients`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const result = await response.json();

        if (result.success) {
            showToast(`Patient ${name} registered successfully!`, result.data.riskLevel === 'High' ? 'danger' : 'success');
            openPatientModal(result.data.id, result.triageEvaluation);
            document.getElementById('intakeForm').reset();
            handleGenderChange();
            await loadPatients();
        } else {
            showToast('Error registering patient', 'danger');
        }
    } catch (err) {
        console.warn('Network request failed, switching to offline local storage:', err);
        saveOfflineRecord(payload);
    }
}

function saveOfflineRecord(payload) {
    try {
        let offlineQueue = JSON.parse(localStorage.getItem('arogya_offline_queue') || '[]');
        offlineQueue.push({
            ...payload,
            capturedAt: new Date().toISOString()
        });
        localStorage.setItem('arogya_offline_queue', JSON.stringify(offlineQueue));

        showToast(`⟳ Offline Mode — Patient ${payload.name} saved locally [UUID: ${payload.localRecordId.substring(0, 8)}]. Pending sync.`, 'warning');
        document.getElementById('intakeForm')?.reset();
        handleGenderChange();
        updateOfflineBadge();
    } catch (e) {
        showToast('Error saving offline record locally', 'danger');
    }
}

async function syncOfflineQueue() {
    let queue = JSON.parse(localStorage.getItem('arogya_offline_queue') || '[]');
    if (queue.length === 0) {
        showToast('No offline records pending synchronization.', 'success');
        return;
    }

    try {
        showToast(`Syncing ${queue.length} offline patient record(s)...`, 'warning');

        const syncPayload = {
            deviceId: 'ASHA-HANDHELD-01',
            records: queue
        };

        const response = await fetch(`${API_BASE}/sync`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(syncPayload)
        });

        const result = await response.json();
        if (result.success) {
            const data = result.data;
            localStorage.removeItem('arogya_offline_queue');
            updateOfflineBadge();
            showToast(`✓ Synchronization complete! ${data.synced} synced, ${data.conflicts} conflicts.`, 'success');
            await loadPatients();
            if (state.currentPage === 'doctor') renderTriageQueueList();
        } else {
            showToast('Sync failed: ' + result.message, 'danger');
        }
    } catch (err) {
        showToast('Sync server unreachable. Please check network connection.', 'danger');
    }
}

function updateOfflineBadge() {
    let queue = JSON.parse(localStorage.getItem('arogya_offline_queue') || '[]');
    const badge = document.getElementById('offlineQueueBtn');
    if (badge) {
        if (queue.length > 0) {
            badge.style.display = 'inline-flex';
            badge.innerText = `⟳ Sync Offline Records (${queue.length})`;
        } else {
            badge.style.display = 'none';
        }
    }
}

// ==========================================================================
// 3. Doctor Dashboard Module & Chart.js Analytics
// ==========================================================================
function renderDoctor() {
    return `
    <div class="card">
        <div class="card-header-title">
            <span>${t('doctorDashboardHeader')}</span>
            <span class="badge badge-status">Live Synchronization</span>
        </div>
        <div class="card-subtitle">${t('doctorDashboardSub')}</div>

        <!-- Filter Bar -->
        <div style="display: flex; gap: 1rem; flex-wrap: wrap; align-items: center; justify-content: space-between; margin-bottom: 1.5rem;">
            <div style="display: flex; gap: 0.5rem; flex-wrap: wrap;">
                <button class="btn btn-outline btn-sm filter-btn active" onclick="setRiskFilter('all', this)">${t('filterAll')}</button>
                <button class="btn btn-outline btn-sm filter-btn" onclick="setRiskFilter('High', this)">${t('filterHigh')}</button>
                <button class="btn btn-outline btn-sm filter-btn" onclick="setRiskFilter('Medium', this)">${t('filterMedium')}</button>
                <button class="btn btn-outline btn-sm filter-btn" onclick="setRiskFilter('Low', this)">${t('filterLow')}</button>
            </div>

            <div style="display: flex; gap: 0.75rem; flex-wrap: wrap; flex: 1; max-width: 500px;">
                <select class="form-select" id="villageFilter" style="flex: 1;" onchange="setVillageFilter(this.value)">
                    <option value="all">${t('allVillages')}</option>
                    <option value="Nandpur">Nandpur</option>
                    <option value="Laxmipur">Laxmipur</option>
                    <option value="Rampur">Rampur</option>
                    <option value="Devpur">Devpur</option>
                </select>
                <input type="text" class="form-input" style="flex: 1.5;" placeholder="${t('searchPlaceholder')}" oninput="setSearchFilter(this.value)">
            </div>
        </div>
    </div>

    <!-- Analytics Charts Grid -->
    <div class="grid grid-2" style="margin-bottom: 1.5rem;">
        <div class="card" style="margin-bottom: 0;">
            <h3 style="margin-bottom: 1rem; font-size: 1.1rem;">📊 Risk Stratification Breakdown</h3>
            <div style="height: 240px; position: relative;">
                <canvas id="riskDoughnutChart"></canvas>
            </div>
        </div>
        <div class="card" style="margin-bottom: 0;">
            <h3 style="margin-bottom: 1rem; font-size: 1.1rem;">🗺️ High Risk Cases by Village Cluster</h3>
            <div style="height: 240px; position: relative;">
                <canvas id="villageBarChart"></canvas>
            </div>
        </div>
    </div>

    <!-- Patient Cards Queue -->
    <div id="doctorQueueContainer">
        <div style="text-align: center; padding: 3rem;">
            <div class="spinner"></div>
            <p style="color: var(--text-muted); margin-top: 1rem;">Loading triage queue...</p>
        </div>
    </div>
    `;
}

function setRiskFilter(risk, btn) {
    state.queueFilter.risk = risk;
    document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
    if (btn) btn.classList.add('active');
    renderTriageQueueList();
}

function setVillageFilter(village) {
    state.queueFilter.village = village;
    renderTriageQueueList();
}

function setSearchFilter(query) {
    state.queueFilter.search = query.toLowerCase();
    renderTriageQueueList();
}

async function loadTriageQueue() {
    await loadPatients();
    initDashboardCharts();
    renderTriageQueueList();
}

function renderTriageQueueList() {
    const container = document.getElementById('doctorQueueContainer');
    if (!container) return;

    let filtered = state.patients.filter(p => {
        if (state.queueFilter.risk !== 'all' && p.riskLevel.toLowerCase() !== state.queueFilter.risk.toLowerCase()) return false;
        if (state.queueFilter.village !== 'all' && p.village.toLowerCase() !== state.queueFilter.village.toLowerCase()) return false;
        if (state.queueFilter.search) {
            const q = state.queueFilter.search;
            const matchName = p.name.toLowerCase().includes(q);
            const matchVillage = p.village.toLowerCase().includes(q);
            const matchSymptoms = p.symptoms && p.symptoms.toLowerCase().includes(q);
            if (!matchName && !matchVillage && !matchSymptoms) return false;
        }
        return true;
    });

    if (filtered.length === 0) {
        container.innerHTML = `
        <div class="card" style="text-align: center; padding: 3rem;">
            <p style="font-size: 1.1rem; color: var(--text-muted);">No patient records found matching current filters.</p>
        </div>`;
        return;
    }

    let html = '';
    filtered.forEach(p => {
        const borderClass = p.riskLevel === 'High' ? 'border-high' : p.riskLevel === 'Medium' ? 'border-medium' : 'border-low';
        const badgeClass = p.riskLevel === 'High' ? 'badge-high' : p.riskLevel === 'Medium' ? 'badge-medium' : 'badge-low';
        
        const genderText = p.gender || 'Female';
        const genderIcon = genderText === 'Female' ? '♀ Female' : genderText === 'Male' ? '♂ Male' : genderText;
        const bloodGroupText = p.bloodGroup && p.bloodGroup !== 'Unknown' ? ` • 🩸 Blood: ${p.bloodGroup}` : '';

        // Obstetric status icon ONLY for female patients
        const isFemale = (genderText === 'Female');
        const pregBadge = isFemale 
            ? (p.isPregnant ? '<span style="font-size: 0.85rem; background: #fbcfe8; color: #9d174d; padding: 0.25rem 0.5rem; border-radius: 4px; font-weight: 600;" title="Pregnant Patient">🤰 Pregnant</span>' : '<span style="font-size: 0.8rem; color: var(--text-muted);">Non-pregnant</span>')
            : '';

        html += `
        <div class="patient-card ${borderClass}">
            <div class="patient-main-info" style="flex: 2;">
                <h3 style="display: flex; align-items: center; gap: 0.6rem; flex-wrap: wrap;">
                    <span>${p.name}</span>
                    ${pregBadge}
                </h3>
                <div class="patient-meta">
                    📍 <strong>${p.village}</strong> • ${genderIcon} • Age ${p.age}${bloodGroupText} • Registered: ${new Date(p.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </div>
                <div class="patient-vitals-strip">
                    <div class="vital-tag"><span class="vital-tag-label">BP:</span><span class="vital-tag-val">${p.vitals.bp}</span></div>
                    <div class="vital-tag"><span class="vital-tag-label">SpO2:</span><span class="vital-tag-val">${p.vitals.spO2}%</span></div>
                    <div class="vital-tag"><span class="vital-tag-label">Temp:</span><span class="vital-tag-val">${p.vitals.temp}°C</span></div>
                    <div class="vital-tag"><span class="vital-tag-label">Glucose:</span><span class="vital-tag-val">${p.vitals.glucose} mg/dL</span></div>
                </div>
                <div style="font-size: 0.88rem; margin-top: 0.5rem; color: var(--text-muted);">
                    <strong>Symptoms:</strong> ${p.symptoms || 'None recorded'}
                </div>
            </div>

            <div style="text-align: right; display: flex; flex-direction: column; align-items: flex-end; gap: 0.5rem;">
                <span class="badge ${badgeClass}" style="font-size: 0.9rem; padding: 0.4rem 0.9rem;">
                    ${p.riskLevel} Risk (${p.riskScore} PTS)
                </span>
                
                <div style="font-size: 0.82rem; font-weight: 600; color: var(--text-muted);">
                    Status: <span style="color: var(--primary-dark);">${p.status || 'Pending'}</span>
                </div>

                <div style="display: flex; gap: 0.5rem; margin-top: 0.5rem;">
                    <button class="btn btn-outline btn-sm" onclick="openPatientModal(${p.id})">
                        🔍 Details
                    </button>
                    ${p.riskLevel === 'High' ? `
                    <button class="btn btn-primary btn-sm" onclick="resendAlert(${p.id})">
                        📲 Alert Doctor
                    </button>` : ''}
                    <button class="btn btn-outline btn-sm" style="color: var(--danger); border-color: #fca5a5;" onclick="deletePatient(${p.id})">
                        🗑️ Delete
                    </button>
                </div>
            </div>
        </div>`;
    });

    container.innerHTML = html;
}

function initDashboardCharts() {
    const doughnutCtx = document.getElementById('riskDoughnutChart')?.getContext('2d');
    const barCtx = document.getElementById('villageBarChart')?.getContext('2d');

    if (!doughnutCtx || !barCtx) return;

    if (state.healthChartInstance) state.healthChartInstance.destroy();
    if (state.villageChartInstance) state.villageChartInstance.destroy();

    const highCount = state.patients.filter(p => p.riskLevel === 'High').length;
    const medCount = state.patients.filter(p => p.riskLevel === 'Medium').length;
    const lowCount = state.patients.filter(p => p.riskLevel === 'Low').length;

    state.healthChartInstance = new Chart(doughnutCtx, {
        type: 'doughnut',
        data: {
            labels: ['High Risk', 'Medium Risk', 'Low Risk'],
            datasets: [{
                data: [highCount, medCount, lowCount],
                backgroundColor: ['#dc2626', '#ea580c', '#16a34a'],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom' } }
        }
    });

    // Village Breakdown
    const villages = ['Nandpur', 'Laxmipur', 'Rampur', 'Devpur'];
    const villageHighCounts = villages.map(v => state.patients.filter(p => p.village === v && p.riskLevel === 'High').length);

    state.villageChartInstance = new Chart(barCtx, {
        type: 'bar',
        data: {
            labels: villages,
            datasets: [{
                label: 'High Risk Patients',
                data: villageHighCounts,
                backgroundColor: '#0d9488',
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } },
            plugins: { legend: { display: false } }
        }
    });
}

// Modal Details & Actions
async function openPatientModal(id, evalData = null) {
    const patient = state.patients.find(p => p.id === id);
    if (!patient && !evalData) return;

    const p = patient;
    const modalTitle = document.getElementById('modalTitle');
    const modalBody = document.getElementById('modalBody');

    modalTitle.innerText = `Patient #${p.id} — ${p.name}`;

    const genderText = p.gender || 'Female';
    const isFemale = (genderText === 'Female');
    const obstetricHtml = isFemale 
        ? `<div><strong>Obstetric Status:</strong> ${p.isPregnant ? 'Pregnant 🤰' : 'Non-pregnant'}</div>`
        : `<div><strong>Obstetric Status:</strong> N/A (Male)</div>`;

    modalBody.innerHTML = `
    <div style="display: flex; flex-direction: column; gap: 1.25rem;">
        <div class="grid grid-2" style="background: var(--bg-app); padding: 1rem; border-radius: var(--radius-md);">
            <div><strong>Village:</strong> ${p.village}</div>
            <div><strong>Age & Gender:</strong> ${p.age} years (${genderText})</div>
            <div><strong>Blood Group:</strong> ${p.bloodGroup || 'Unknown'}</div>
            ${obstetricHtml}
            <div><strong>Registered:</strong> ${new Date(p.timestamp).toLocaleString()}</div>
        </div>

        <div>
            <h4 style="margin-bottom: 0.5rem; color: var(--primary-dark);">Vital Signs Record</h4>
            <div class="grid grid-4">
                <div style="background: white; border: 1px solid var(--border-color); padding: 0.75rem; border-radius: var(--radius-sm); text-align: center;">
                    <div style="font-size: 0.8rem; color: var(--text-muted);">BP</div>
                    <div style="font-weight: 700; font-size: 1.1rem;">${p.vitals.bp}</div>
                </div>
                <div style="background: white; border: 1px solid var(--border-color); padding: 0.75rem; border-radius: var(--radius-sm); text-align: center;">
                    <div style="font-size: 0.8rem; color: var(--text-muted);">SpO2</div>
                    <div style="font-weight: 700; font-size: 1.1rem;">${p.vitals.spO2}%</div>
                </div>
                <div style="background: white; border: 1px solid var(--border-color); padding: 0.75rem; border-radius: var(--radius-sm); text-align: center;">
                    <div style="font-size: 0.8rem; color: var(--text-muted);">Temp</div>
                    <div style="font-weight: 700; font-size: 1.1rem;">${p.vitals.temp}°C</div>
                </div>
                <div style="background: white; border: 1px solid var(--border-color); padding: 0.75rem; border-radius: var(--radius-sm); text-align: center;">
                    <div style="font-size: 0.8rem; color: var(--text-muted);">Glucose</div>
                    <div style="font-weight: 700; font-size: 1.1rem;">${p.vitals.glucose}</div>
                </div>
            </div>
        </div>

        <div style="background: #f8fafc; border-left: 4px solid var(--primary); padding: 1rem; border-radius: var(--radius-sm);">
            <strong>Reported Symptoms:</strong>
            <p style="margin-top: 0.25rem; font-size: 0.95rem;">${p.symptoms || 'No specific symptoms entered.'}</p>
        </div>

        <div>
            <h4 style="margin-bottom: 0.5rem;">Update Doctor Status & Notes</h4>
            <div class="grid grid-2">
                <div class="form-group" style="margin-bottom: 0;">
                    <label class="form-label">Triage Status</label>
                    <select class="form-select" id="modalStatusSelect">
                        <option value="Pending" ${p.status === 'Pending' ? 'selected' : ''}>Pending Review</option>
                        <option value="Referred to CHC" ${p.status === 'Referred to CHC' ? 'selected' : ''}>Referred to CHC / Hospital</option>
                        <option value="Under Observation" ${p.status === 'Under Observation' ? 'selected' : ''}>Under Observation</option>
                        <option value="Discharged" ${p.status === 'Discharged' ? 'selected' : ''}>Discharged</option>
                    </select>
                </div>
                <div class="form-group" style="margin-bottom: 0;">
                    <label class="form-label">Doctor Notes</label>
                    <input type="text" class="form-input" id="modalNotesInput" value="${p.doctorNotes || ''}" placeholder="Enter clinical notes...">
                </div>
            </div>
        </div>

        <div style="display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1rem;">
            <button class="btn btn-outline" onclick="closeModal()">Close</button>
            <button class="btn btn-primary" onclick="savePatientStatus(${p.id})">Save Updates</button>
        </div>
    </div>`;

    document.getElementById('appModal').classList.add('active');
}

async function savePatientStatus(id) {
    const status = document.getElementById('modalStatusSelect').value;
    const doctorNotes = document.getElementById('modalNotesInput').value;

    try {
        const response = await fetch(`${API_BASE}/patients/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status, doctorNotes })
        });

        const result = await response.json();
        if (result.success) {
            showToast('Patient status updated successfully!', 'success');
            closeModal();
            await loadPatients();
            renderTriageQueueList();
        }
    } catch (err) {
        showToast('Error updating status', 'danger');
    }
}

async function deletePatient(id) {
    if (!confirm(`Are you sure you want to remove patient record #${id}?`)) return;

    try {
        const response = await fetch(`${API_BASE}/patients/${id}`, { method: 'DELETE' });
        const result = await response.json();
        if (result.success) {
            showToast('Patient removed from triage queue.', 'warning');
            await loadPatients();
            renderTriageQueueList();
        }
    } catch (err) {
        showToast('Error deleting patient', 'danger');
    }
}

function resendAlert(id) {
    showToast(`📲 WhatsApp alert sent to District Doctor for Patient #${id}`, 'success');
}

// ==========================================================================
// 4. ASHA Bot AI Assistant Module
// ==========================================================================
function renderBot() {
    return `
    <div class="card">
        <div class="card-header-title">
            <span>${t('botHeader')}</span>
            <span class="badge badge-status">Natural Language Extraction Engine</span>
        </div>
        <div class="card-subtitle">${t('botSub')}</div>

        <!-- Multilingual Suggestion Chips -->
        <div style="display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1.25rem;">
            <button class="btn btn-outline btn-sm" onclick="sendPromptChip('Patient ku moochu kashtama irukku, oxygen 88')">🇮🇳 Tamil: Mooschu Kashtama (SpO2 88%)</button>
            <button class="btn btn-outline btn-sm" onclick="sendPromptChip('मरीज का बीपी 160/100 है, सांस फूल रही है')">🇮🇳 Hindi: BP 160/100 High Risk</button>
            <button class="btn btn-outline btn-sm" onclick="sendPromptChip('What to do if SpO2 is below 90%?')">${t('chipHypoxia')}</button>
            <button class="btn btn-outline btn-sm" onclick="sendPromptChip('Pre-eclampsia warning signs in pregnancy')">${t('chipPreeclampsia')}</button>
        </div>

        <div id="chatBox" style="background: var(--bg-app); border: 1px solid var(--border-color); border-radius: var(--radius-md); padding: 1.25rem; height: 420px; overflow-y: auto; margin-bottom: 1.25rem; display: flex; flex-direction: column; gap: 1rem;">
            <div style="background: white; border: 1px solid var(--border-color); padding: 1rem; border-radius: var(--radius-md); max-width: 85%;">
                <strong>🤖 ASHA Bot AI:</strong>
                <p style="margin-top: 0.25rem;">Hello! I am your AI Clinical Assistant. Type or paste ASHA observations in <strong>English, Hindi, Tamil, or Hinglish</strong> (e.g. <em>"Patient ku moochu kashtama irukku, oxygen 88"</em>). I will extract vitals, symptoms, and run automated triage scoring.</p>
            </div>
        </div>

        <div style="display: flex; gap: 0.75rem;">
            <input type="text" class="form-input" id="chatInput" placeholder="${t('chatPlaceholder')}" style="flex: 1;" onkeypress="if(event.key==='Enter') sendChatMessage()">
            <button class="btn btn-primary" onclick="sendChatMessage()">${t('sendBtn')}</button>
        </div>
    </div>
    `;
}

function setupBotChat() {
    const input = document.getElementById('chatInput');
    if (input) input.focus();
}

function sendPromptChip(text) {
    const input = document.getElementById('chatInput');
    if (input) {
        input.value = text;
        sendChatMessage();
    }
}

async function sendChatMessage() {
    const input = document.getElementById('chatInput');
    const query = input.value.trim();
    if (!query) return;

    const chatBox = document.getElementById('chatBox');

    // Append User Message
    const userDiv = document.createElement('div');
    userDiv.style.cssText = 'align-self: flex-end; background: var(--primary); color: white; padding: 0.85rem 1.15rem; border-radius: var(--radius-md); max-width: 80%; font-weight: 500;';
    userDiv.innerText = query;
    chatBox.appendChild(userDiv);

    input.value = '';
    chatBox.scrollTop = chatBox.scrollHeight;

    // Typing Indicator
    const typingDiv = document.createElement('div');
    typingDiv.style.cssText = 'background: white; border: 1px solid var(--border-color); padding: 0.75rem 1rem; border-radius: var(--radius-md); max-width: 80%; color: var(--text-muted);';
    typingDiv.innerText = '🤖 ASHA Bot AI is extracting vitals & analyzing clinical protocols...';
    chatBox.appendChild(typingDiv);
    chatBox.scrollTop = chatBox.scrollHeight;

    try {
        const response = await fetch(`${API_BASE}/chat`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ query, language: state.language })
        });

        const result = await response.json();
        chatBox.removeChild(typingDiv);

        if (result.success) {
            const data = result.data;
            const botDiv = document.createElement('div');
            const borderColor = data.severity === 'Critical' ? 'var(--danger)' : data.severity === 'Warning' ? 'var(--warning)' : 'var(--primary)';
            botDiv.style.cssText = `background: white; border: 1px solid var(--border-color); border-left: 5px solid ${borderColor}; padding: 1.1rem; border-radius: var(--radius-md); max-width: 88%;`;

            let metaBadges = `<div style="display: flex; gap: 0.4rem; flex-wrap: wrap; margin-bottom: 0.6rem;">`;
            metaBadges += `<span class="badge badge-status" style="font-size: 0.78rem;">🌐 Language: ${data.languageDetected || 'Detected'}</span>`;

            if (data.extractedVitals) {
                const v = data.extractedVitals;
                let vitalsText = [];
                if (v.spO2 > 0) vitalsText.push(`SpO2: ${v.spO2}%`);
                if (v.bp) vitalsText.push(`BP: ${v.bp}`);
                if (v.temp > 0 && v.temp !== 37) vitalsText.push(`Temp: ${v.temp}°C`);
                if (v.glucose > 0 && v.glucose !== 100) vitalsText.push(`Glucose: ${v.glucose}`);
                metaBadges += `<span class="badge badge-high" style="font-size: 0.78rem; background: #e0f2fe; color: #0369a1;">🩺 Vitals Extracted: ${vitalsText.join(' • ')}</span>`;
            }

            if (data.extractedSymptoms && data.extractedSymptoms.length > 0) {
                metaBadges += `<span class="badge" style="font-size: 0.78rem; background: #f1f5f9; color: #334155;">🏷️ Symptoms: ${data.extractedSymptoms.join(', ')}</span>`;
            }

            if (data.triageEvaluation) {
                const tr = data.triageEvaluation;
                const trBadgeClass = tr.riskLevel === 'High' ? 'badge-high' : tr.riskLevel === 'Medium' ? 'badge-medium' : 'badge-low';
                metaBadges += `<span class="badge ${trBadgeClass}" style="font-size: 0.78rem;">⚡ Triage: ${tr.riskLevel.toUpperCase()} RISK (${tr.totalScore} PTS)</span>`;
            }
            metaBadges += `</div>`;

            let actionsHtml = data.actionSteps && data.actionSteps.length > 0 
                ? `<div style="margin-top: 0.75rem; background: #f8fafc; padding: 0.75rem; border-radius: var(--radius-sm); border: 1px solid #e2e8f0;"><strong>Action Steps:</strong><ul style="padding-left: 1.25rem; margin-top: 0.25rem; font-size: 0.9rem;">${data.actionSteps.map(a => `<li>${a}</li>`).join('')}</ul></div>` 
                : '';

            let disclaimerHtml = data.disclaimer 
                ? `<div style="margin-top: 0.75rem; font-size: 0.78rem; color: var(--text-muted); font-style: italic; border-top: 1px solid #e2e8f0; padding-top: 0.5rem;">⚠️ Disclaimer: ${data.disclaimer}</div>`
                : '';

            botDiv.innerHTML = `
            ${metaBadges}
            <strong style="color: var(--primary-dark);">🤖 ASHA Bot AI Guidance:</strong>
            <div style="margin-top: 0.4rem; white-space: pre-line; line-height: 1.5;">${data.response}</div>
            ${actionsHtml}
            ${disclaimerHtml}`;

            chatBox.appendChild(botDiv);
        }
    } catch (err) {
        chatBox.removeChild(typingDiv);
        showToast('Error connecting to AI Chat Service', 'danger');
    }

    chatBox.scrollTop = chatBox.scrollHeight;
}

// ==========================================================================
// 5. Village Clusters Module
// ==========================================================================
function renderVillages() {
    const stats = state.villageStats.length > 0 ? state.villageStats : [
        { village: 'Nandpur', totalScreened: 245, highRiskCount: 12, primaryHealthCenter: 'Nandpur Health Center' },
        { village: 'Laxmipur', totalScreened: 189, highRiskCount: 8, primaryHealthCenter: 'Laxmipur PHC' },
        { village: 'Rampur', totalScreened: 156, highRiskCount: 5, primaryHealthCenter: 'Rampur Sub-Hospital' },
        { village: 'Devpur', totalScreened: 203, highRiskCount: 14, primaryHealthCenter: 'Devpur Rural Center' }
    ];

    let rowsHtml = '';
    stats.forEach(v => {
        const ratio = v.totalScreened > 0 ? Math.round((v.highRiskCount / v.totalScreened) * 100) : 0;
        rowsHtml += `
        <tr>
            <td><strong>${v.village}</strong></td>
            <td>${v.totalScreened}</td>
            <td><span class="badge badge-high">${v.highRiskCount} High Risk</span></td>
            <td>
                <div style="display: flex; align-items: center; gap: 0.5rem;">
                    <div style="flex: 1; height: 8px; background: #e2e8f0; border-radius: 4px; overflow: hidden;">
                        <div style="width: ${ratio * 3}%; height: 100%; background: var(--danger);"></div>
                    </div>
                    <span>${ratio}%</span>
                </div>
            </td>
            <td>${v.primaryHealthCenter}</td>
            <td>
                <button class="btn btn-outline btn-sm" onclick="filterQueueByVillage('${v.village}')">
                    ${t('filterQueueBtn')}
                </button>
            </td>
        </tr>`;
    });

    return `
    <div class="card">
        <div class="card-header-title">${t('villagesHeader')}</div>
        <div class="card-subtitle">${t('villagesSub')}</div>

        <div class="table-responsive">
            <table class="table">
                <thead>
                    <tr>
                        <th>${t('colVillage')}</th>
                        <th>${t('colScreened')}</th>
                        <th>${t('colHighRisk')}</th>
                        <th>${t('colRatio')}</th>
                        <th>${t('colPHC')}</th>
                        <th>${t('colAction')}</th>
                    </tr>
                </thead>
                <tbody>
                    ${rowsHtml}
                </tbody>
            </table>
        </div>
    </div>`;
}

function filterQueueByVillage(villageName) {
    state.queueFilter.village = villageName;
    renderPage('doctor');
}

// ==========================================================================
// 6. Triage Engine Simulator Module
// ==========================================================================
function renderAlgorithm() {
    return `
    <div class="card">
        <div class="card-header-title">${t('engineHeader')}</div>
        <div class="card-subtitle">${t('engineSub')}</div>
    </div>

    <!-- Simulator -->
    <div class="card" style="border-left: 6px solid var(--primary);">
        <h3 style="margin-bottom: 0.5rem;">${t('simTitle')}</h3>
        <p style="color: var(--text-muted); margin-bottom: 1.5rem;">${t('simSub')}</p>

        <div class="grid grid-2">
            <div>
                <div class="form-group">
                    <label class="form-label">Systolic BP: <span id="simSysVal">140</span> mmHg</label>
                    <input type="range" min="90" max="220" value="140" class="form-input sim-trigger" id="simSys">
                </div>
                <div class="form-group">
                    <label class="form-label">Diastolic BP: <span id="simDiaVal">90</span> mmHg</label>
                    <input type="range" min="50" max="130" value="90" class="form-input sim-trigger" id="simDia">
                </div>
                <div class="form-group">
                    <label class="form-label">SpO2 Oxygen: <span id="simSpo2Val">92</span> %</label>
                    <input type="range" min="70" max="100" value="92" class="form-input sim-trigger" id="simSpo2">
                </div>
            </div>

            <div>
                <div class="form-group">
                    <label class="form-label">Glucose: <span id="simGlucoseVal">110</span> mg/dL</label>
                    <input type="range" min="40" max="350" value="110" class="form-input sim-trigger" id="simGlucose">
                </div>
                <div class="form-group">
                    <label class="form-label">Temperature: <span id="simTempVal">37.0</span> °C</label>
                    <input type="range" min="34" max="41" step="0.1" value="37.0" class="form-input sim-trigger" id="simTemp">
                </div>
                <div class="form-group">
                    <label class="form-label" style="cursor: pointer;">
                        <input type="checkbox" id="simPregnant" class="sim-trigger"> Is Pregnant Patient? (Female)
                    </label>
                </div>
            </div>
        </div>

        <div class="live-calculator-box" style="margin-top: 1rem;">
            <div class="calc-header">
                <span class="calc-title">Simulator Calculated Risk Result:</span>
                <span class="calc-score-badge badge badge-medium" id="simBadge">0 PTS • LOW RISK</span>
            </div>
            <div id="simBreakdown" style="font-size: 0.9rem;"></div>
        </div>
    </div>`;
}

function setupSimulatorListeners() {
    document.querySelectorAll('.sim-trigger').forEach(el => {
        el.addEventListener('input', runSimulatorCalc);
        el.addEventListener('change', runSimulatorCalc);
    });
    runSimulatorCalc();
}

function runSimulatorCalc() {
    const sys = parseInt(document.getElementById('simSys')?.value) || 120;
    const dia = parseInt(document.getElementById('simDia')?.value) || 80;
    const spo2 = parseInt(document.getElementById('simSpo2')?.value) || 98;
    const glucose = parseInt(document.getElementById('simGlucose')?.value) || 100;
    const temp = parseFloat(document.getElementById('simTemp')?.value) || 37.0;
    const isPregnant = document.getElementById('simPregnant')?.checked || false;

    if (document.getElementById('simSysVal')) document.getElementById('simSysVal').innerText = sys;
    if (document.getElementById('simDiaVal')) document.getElementById('simDiaVal').innerText = dia;
    if (document.getElementById('simSpo2Val')) document.getElementById('simSpo2Val').innerText = spo2;
    if (document.getElementById('simGlucoseVal')) document.getElementById('simGlucoseVal').innerText = glucose;
    if (document.getElementById('simTempVal')) document.getElementById('simTempVal').innerText = temp;

    let score = 0;
    let points = [];

    if (sys >= 160 || dia >= 100) { score += 40; points.push(`🔴 Hypertensive Crisis BP (${sys}/${dia}): +40 pts`); }
    else if (sys >= 140 || dia >= 90) { score += 20; points.push(`🟠 Stage 1 Hypertension BP (${sys}/${dia}): +20 pts`); }

    if (isPregnant && (sys >= 140 || dia >= 90)) { score += 35; points.push(`🤰 Pre-Eclampsia Obstetric Warning: +35 pts`); }

    if (spo2 < 90) { score += 35; points.push(`🔴 Severe Hypoxia (SpO2 ${spo2}%): +35 pts`); }
    else if (spo2 <= 94) { score += 15; points.push(`🟠 Moderate Hypoxemia (SpO2 ${spo2}%): +15 pts`); }

    if (glucose >= 200 || glucose < 70) { score += 30; points.push(`🟠 Glucose Anomaly (${glucose} mg/dL): +30 pts`); }
    if (temp >= 39.0 || temp < 35.0) { score += 20; points.push(`🟠 Temp Anomaly (${temp}°C): +20 pts`); }

    let riskLevel = 'Low';
    let badgeClass = 'badge-low';
    if (score >= 45) { riskLevel = 'High'; badgeClass = 'badge-high'; }
    else if (score >= 20) { riskLevel = 'Medium'; badgeClass = 'badge-medium'; }

    const simBadge = document.getElementById('simBadge');
    const simBreakdown = document.getElementById('simBreakdown');

    if (simBadge) {
        simBadge.className = `calc-score-badge badge ${badgeClass}`;
        simBadge.innerText = `${score} PTS • ${riskLevel.toUpperCase()} RISK`;
    }

    if (simBreakdown) {
        simBreakdown.innerHTML = points.length > 0 
            ? `<ul style="padding-left: 1.25rem;">${points.map(p => `<li>${p}</li>`).join('')}</ul>` 
            : `<span style="color: var(--success); font-weight: 600;">✓ Physiological parameters fully normal (0 Points).</span>`;
    }
}

// ==========================================================================
// Helper Services & API Data Loaders
// ==========================================================================
async function loadPatients() {
    try {
        const response = await fetch(`${API_BASE}/patients`);
        const result = await response.json();
        if (result.success) state.patients = result.data;
    } catch (err) {
        console.error('Error loading patients:', err);
    }
}

async function loadSystemStats() {
    try {
        const response = await fetch(`${API_BASE}/stats`);
        const result = await response.json();
        if (result.success) state.systemStats = result.data;
    } catch (err) {
        console.error('Error loading stats:', err);
    }
}

async function loadVillageStats() {
    try {
        const response = await fetch(`${API_BASE}/villages`);
        const result = await response.json();
        if (result.success) state.villageStats = result.data;
    } catch (err) {
        console.error('Error loading village stats:', err);
    }
}

function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `<span>${message}</span>`;
    container.appendChild(toast);

    setTimeout(() => toast.remove(), 4000);
}

function closeModal() {
    const modal = document.getElementById('appModal');
    if (modal) modal.classList.remove('active');
}