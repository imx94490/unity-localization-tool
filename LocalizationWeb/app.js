(() => {
  let workbook = null;
  let headers = [];
  let records = [];
  const fileInput = document.getElementById('fileInput');
  const btnParse = document.getElementById('btnParse');
  const langSelect = document.getElementById('langSelect');
  const btnPreview = document.getElementById('btnPreview');
  const btnValidate = document.getElementById('btnValidate');
  const btnExportDir = document.getElementById('btnExportDir');
  const btnExportAll = document.getElementById('btnExportAll');
  const jsonOutput = document.getElementById('jsonOutput');
  const status = document.getElementById('status');
  const btnDownloadTemplate = document.getElementById('btnDownloadTemplate');

  function setStatus(s) { status.textContent = s || ''; }
  function parseSheet(data) {
    const wb = XLSX.read(data, { type: 'array' });
    let wsname = 'Localization';
    if (!wb.SheetNames.includes(wsname)) wsname = wb.SheetNames[0];
    const ws = wb.Sheets[wsname];
    const rows = XLSX.utils.sheet_to_json(ws, { header: 1, defval: '' });
    if (!rows || rows.length === 0) return null;
    const h = rows[0].map(x => String(x).trim());
    const keyIdx = h.indexOf('Key');
    if (keyIdx < 0) return null;
    const langs = h.filter(x => x !== 'Key' && x !== 'Comment' && x !== '备注');
    const recs = [];
    for (let r = 1; r < rows.length; r++) {
      const row = rows[r];
      const k = String(row[keyIdx] || '').trim();
      if (!k) continue;
      const item = { Key: k };
      for (let li = 0; li < langs.length; li++) {
        const colIdx = h.indexOf(langs[li]);
        item[langs[li]] = String(row[colIdx] || '').trim();
      }
      recs.push(item);
    }
    return { wb, headers: h, languages: langs, records: recs };
  }

  function buildJson(lang) {
    const data = {};
    for (let i = 0; i < records.length; i++) {
      const k = records[i].Key;
      const v = records[i][lang];
      if (k) data[k] = v != null ? String(v) : '';
    }
    return data;
  }

  function validateJson(obj) {
    if (!obj) return { ok: false, errors: ['Empty data'] };
    const errors = [];
    const ks = Object.keys(obj);
    const seen = new Set();
    for (let i = 0; i < ks.length; i++) {
      const k = ks[i];
      if (!k) errors.push('Empty key');
      if (seen.has(k)) errors.push('Duplicate key: ' + k);
      seen.add(k);
      const v = obj[k];
      if (typeof v !== 'string') errors.push('Non-string value: ' + k);
    }
    let s = '';
    try { s = JSON.stringify(obj); JSON.parse(s); }
    catch (e) { errors.push('JSON stringify/parse failed'); }
    return { ok: errors.length === 0, errors };
  }

  async function pickDirectory() {
    if (!window.showDirectoryPicker) return null;
    try { const dir = await window.showDirectoryPicker(); return dir; }
    catch (e) { return null; }
  }

  async function writeFile(dir, name, text) {
    const h = await dir.getFileHandle(name, { create: true });
    const w = await h.createWritable();
    await w.write(text);
    await w.close();
  }

  async function exportLang(lang) {
    const obj = buildJson(lang);
    const v = validateJson(obj);
    if (!v.ok) { setStatus('校验失败\n' + v.errors.join('\n')); return; }
    const dir = await pickDirectory();
    if (!dir) { setStatus('未选择目录'); return; }
    const text = JSON.stringify(obj, null, 2);
    await writeFile(dir, lang + '.json', text);
    setStatus('已导出: ' + lang + '.json');
  }

  async function exportAll() {
    const dir = await pickDirectory();
    if (!dir) { setStatus('未选择目录'); return; }
    for (let i = 0; i < headers.length; i++) {}
    for (let i = 0; i < languages.length; i++) {
      const lang = languages[i];
      const obj = buildJson(lang);
      const v = validateJson(obj);
      if (!v.ok) { setStatus('校验失败: ' + lang + '\n' + v.errors.join('\n')); return; }
      const text = JSON.stringify(obj, null, 2);
      await writeFile(dir, lang + '.json', text);
    }
    setStatus('已导出所有文化');
  }

  function fillLangSelect(langs) {
    langSelect.innerHTML = '';
    for (let i = 0; i < langs.length; i++) {
      const opt = document.createElement('option');
      opt.value = langs[i];
      opt.textContent = langs[i];
      langSelect.appendChild(opt);
    }
  }

  async function handleParse() {
    setStatus('');
    const f = fileInput.files && fileInput.files[0];
    if (!f) { setStatus('请选择xlsx文件'); return; }
    const ab = await f.arrayBuffer();
    const parsed = parseSheet(ab);
    if (!parsed) { setStatus('解析失败'); return; }
    workbook = parsed.wb;
    headers = parsed.headers;
    languages = parsed.languages;
    records = parsed.records;
    fillLangSelect(languages);
    setStatus('解析成功');
  }

  function handlePreview() {
    setStatus('');
    const lang = langSelect.value;
    if (!lang) { setStatus('请选择文化'); return; }
    const obj = buildJson(lang);
    jsonOutput.value = JSON.stringify(obj, null, 2);
  }

  function handleValidate() {
    const s = jsonOutput.value || '';
    let obj = null;
    try { obj = JSON.parse(s); }
    catch (e) { setStatus('JSON格式错误'); return; }
    const v = validateJson(obj);
    setStatus(v.ok ? '校验通过' : ('校验失败\n' + v.errors.join('\n')));
  }

  function templateData() {
    const keys = [
      ['TipStandCenter','请站到镜头中央','Please stand at the center'],
      ['TipClapToStart','拍手准备进入游戏','Clap to start'],
      ['TipReady','已准备','Ready'],
      ['LabelGameFinished','游戏结束','Game Finished'],
      ['LogStartGame','开始游戏','Start Game'],
      ['LogSelectPlayerCount','选择人数','Select Player Count'],
      ['PlayerNameFallback','玩家{0}','Player {0}'],
      ['Verb.Squat','蹲','Squat'],
      ['Verb.Jump','跳','Jump'],
      ['Verb.WaistTwist','扭腰','Waist Twist'],
      ['Verb.BendOver','弯腰','Bend Over'],
      ['Verb.ShakeHead','摇头','Shake Head'],
      ['Verb.Nod','点头','Nod'],
      ['Verb.ClapHands','拍手','Clap Hands'],
      ['Verb.Freeze','不动','Freeze'],
      ['Verb.RaiseHands','举手','Raise Hands'],
      ['Verb.LiftFoot','抬脚','Lift Foot']
    ];
    const rows = [['Key','zh-CN','en-US']];
    for (let i = 0; i < keys.length; i++) rows.push(keys[i]);
    return rows;
  }

  function downloadTemplate() {
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(templateData());
    XLSX.utils.book_append_sheet(wb, ws, 'Localization');
    XLSX.writeFile(wb, 'Language_Template.xlsx');
  }

  btnParse.addEventListener('click', handleParse);
  btnPreview.addEventListener('click', handlePreview);
  btnValidate.addEventListener('click', handleValidate);
  btnExportDir.addEventListener('click', () => { const lang = langSelect.value; if (!lang) { setStatus('请选择文化'); return; } exportLang(lang); });
  btnExportAll.addEventListener('click', exportAll);
  btnDownloadTemplate.addEventListener('click', downloadTemplate);
})();

