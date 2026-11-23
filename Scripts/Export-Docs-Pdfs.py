#!/usr/bin/env python3
"""
Export-Docs-Pdfs.py
-------------------
Converts all Guides and Diagrams in the Learning folder to polished PDF documents
under the MAUI project Docs directory (PlayWithSQLITE/Docs) and repository root Docs,
placing the master assignment inside the Assignment subdirectory.
"""

import os
import sys
import re
import base64
import shutil
from pathlib import Path
from urllib.parse import quote, unquote, urlparse, parse_qs
from markdown_it import MarkdownIt
from pygments import highlight
from pygments.lexers import get_lexer_by_name, TextLexer
from pygments.formatters import HtmlFormatter
from playwright.sync_api import sync_playwright
from pypdf import PdfReader, PdfWriter
from pypdf.generic import ArrayObject, BooleanObject, DictionaryObject, NameObject, NumberObject, TextStringObject

REL_PDF_SENTINEL = "https://rel-pdf.local/?p="

# Ensure UTF-8 output on Windows consoles
if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding="utf-8")
        sys.stderr.reconfigure(encoding="utf-8")
    except Exception:
        pass


def highlight_code(code, name, attrs):
    lang = (name or "").strip().lower()
    if not lang:
        lang = "text"
    try:
        lexer = get_lexer_by_name(lang, stripall=True)
    except Exception:
        lexer = TextLexer()
    formatter = HtmlFormatter(nowrap=True, style="friendly")
    highlighted = highlight(code, lexer, formatter)

    # Support highlighted added lines via // [NEW-START] ... // [NEW-END] or ✨ marker
    if "[NEW-START]" in highlighted or "✨" in highlighted:
        out_lines = []
        in_hl = False
        for line in highlighted.splitlines():
            if "[NEW-START]" in line:
                in_hl = True
                continue
            if "[NEW-END]" in line:
                in_hl = False
                continue
            if in_hl or "✨" in line:
                out_lines.append(f'<span class="hl-added">{line or " "}</span>')
            else:
                out_lines.append(line)
        highlighted = "\n".join(out_lines)

    return f'<div class="code-block" dir="ltr"><div class="code-header"><span class="code-lang">{lang.upper()}</span></div><pre class="highlight"><code>{highlighted}</code></pre></div>'


def transform_callouts(html_text: str) -> str:
    """Converts GitHub-style alerts into attractive callout boxes."""
    patterns = {
        r'<blockquote>\s*<p>\s*\[!TIP\]\s*(.*?)</p>': r'<div class="callout callout-tip"><div class="callout-icon">💡</div><div class="callout-content"><p>\1</p>',
        r'<blockquote>\s*<p>\s*\[!NOTE\]\s*(.*?)</p>': r'<div class="callout callout-note"><div class="callout-icon">ℹ️</div><div class="callout-content"><p>\1</p>',
        r'<blockquote>\s*<p>\s*\[!IMPORTANT\]\s*(.*?)</p>': r'<div class="callout callout-important"><div class="callout-icon">⚠️</div><div class="callout-content"><p>\1</p>',
        r'<blockquote>\s*<p>\s*\[!WARNING\]\s*(.*?)</p>': r'<div class="callout callout-warning"><div class="callout-icon">🚨</div><div class="callout-content"><p>\1</p>',
        r'<blockquote>\s*<p>\s*\[!CAUTION\]\s*(.*?)</p>': r'<div class="callout callout-caution"><div class="callout-icon">🛑</div><div class="callout-content"><p>\1</p>',
    }
    for pat, rep in patterns.items():
        html_text = re.sub(pat, rep, html_text, flags=re.DOTALL)
    html_text = html_text.replace('</blockquote>', '</div></div>')
    return html_text


def embed_images(html_text: str, base_dir: Path, learning_dir: Path) -> str:
    """Embeds local images as inline base64 data URIs."""
    def replace_img(match):
        src = match.group(1)
        alt = match.group(2)
        if src.startswith("http://") or src.startswith("https://") or src.startswith("data:"):
            return match.group(0)

        clean_src = src.replace("../", "").replace("./", "")
        candidates = [
            base_dir / clean_src,
            learning_dir / clean_src,
            learning_dir / "Images" / Path(clean_src).name,
            base_dir / "Images" / Path(clean_src).name
        ]

        found_path = None
        for cand in candidates:
            if cand.exists():
                found_path = cand
                break

        if found_path:
            ext = found_path.suffix.lower().replace(".", "")
            if ext == "jpg":
                ext = "jpeg"
            try:
                with open(found_path, "rb") as f:
                    b64 = base64.b64encode(f.read()).decode("utf-8")
                is_phone = found_path.stem.endswith("-live") or found_path.stem.endswith("-android")
                cls = "embedded-img phone-shot" if is_phone else "embedded-img"
                caption_html = f'<div class="img-caption">{alt}</div>' if alt else ""
                return f'<div class="img-wrap"><img src="data:image/{ext};base64,{b64}" class="{cls}" alt="{alt}">{caption_html}</div>'
            except Exception as e:
                print(f"Warning: Failed embedding image {found_path}: {e}")
        return match.group(0)

    return re.sub(r'<img\s+src="([^"]+)"\s+alt="([^"]*)"[^>]*>', replace_img, html_text)


def build_html_document(title: str, body_html: str, subtitle: str = "") -> str:
    formatter = HtmlFormatter(style="friendly")
    pygments_css = formatter.get_style_defs('.highlight')

    return f"""<!DOCTYPE html>
<html lang="he" dir="rtl">
<head>
  <meta charset="UTF-8">
  <title>{title}</title>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Assistant:wght@300;400;600;700;800&family=Fira+Code:wght@400;500;700&display=swap');

    :root {{
      --primary: #0284c7;
      --primary-dark: #0369a1;
      --secondary: #6366f1;
      --accent: #f59e0b;
      --text: #0f172a;
      --text-muted: #475569;
      --bg: #ffffff;
      --border: #cbd5e1;
    }}

    @page {{
      size: A4;
      margin: 15mm 14mm 16mm 14mm;
      @bottom-left {{
        content: ".NET MAUI & SQLite — תיעוד ומדריכים";
        font-family: 'Assistant', 'Segoe UI', sans-serif;
        font-size: 8.5pt;
        color: #64748b;
        direction: rtl;
      }}
      @bottom-right {{
        content: counter(page);
        font-family: 'Assistant', 'Segoe UI', sans-serif;
        font-size: 8.5pt;
        color: #64748b;
        font-weight: bold;
      }}
    }}

    * {{
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }}

    body {{
      font-family: 'Assistant', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      direction: rtl;
      text-align: right;
      color: var(--text);
      background: var(--bg);
      line-height: 1.6;
      font-size: 10.5pt;
      padding: 5px 2px;
    }}

    .doc-header {{
      border-bottom: 2px solid var(--primary);
      padding-bottom: 12px;
      margin-bottom: 22px;
    }}

    .doc-header h1 {{
      font-size: 22pt;
      color: #0c4a6e;
      font-weight: 800;
      margin-bottom: 4px;
    }}

    .doc-header .doc-subtitle {{
      font-size: 11pt;
      color: var(--text-muted);
      font-weight: 600;
    }}

    h1, h2, h3, h4, h5, h6 {{
      color: #0f172a;
      font-weight: 700;
      page-break-after: avoid;
      break-after: avoid;
    }}

    h1 {{
      font-size: 18pt;
      margin-top: 18px;
      margin-bottom: 12px;
      color: #0369a1;
      border-bottom: 1px solid #e2e8f0;
      padding-bottom: 4px;
    }}

    h2 {{
      font-size: 14pt;
      margin-top: 16px;
      margin-bottom: 8px;
      color: #0284c7;
    }}

    h3 {{
      font-size: 12pt;
      margin-top: 12px;
      margin-bottom: 6px;
      color: #334155;
    }}

    p {{
      margin-bottom: 10px;
    }}

    ul, ol {{
      margin-bottom: 12px;
      padding-right: 22px;
    }}

    li {{
      margin-bottom: 4px;
    }}

    code {{
      font-family: 'Fira Code', Consolas, Monaco, monospace;
      direction: ltr;
      unicode-bidi: isolate;
      font-size: 9pt;
      background: #f1f5f9;
      color: #0369a1;
      padding: 2px 5px;
      border-radius: 4px;
      border: 1px solid #cbd5e1;
    }}

    .code-block {{
      direction: ltr;
      text-align: left;
      margin: 12px 0;
      background: #f8fafc;
      border: 1px solid #cbd5e1;
      border-radius: 8px;
      overflow: hidden;
      break-inside: auto;
      page-break-inside: auto;
    }}

    .code-header {{
      background: #e2e8f0;
      padding: 4px 10px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid #cbd5e1;
    }}

    .code-lang {{
      font-size: 8pt;
      font-weight: 700;
      color: #475569;
      font-family: 'Fira Code', monospace;
    }}

    pre.highlight {{
      margin: 0;
      padding: 10px 12px;
      overflow-x: hidden !important;
      white-space: pre-wrap !important;
      word-break: break-word !important;
      background: #f8fafc !important;
      color: #1e293b;
      font-family: 'Fira Code', Consolas, monospace;
      font-size: 8.5pt;
      line-height: 1.45;
    }}

    .hl-added {{
      display: inline-block;
      width: calc(100% + 24px);
      background: #fef9c3;
      border-left: 3.5px solid #d97706;
      margin: 0 -12px;
      padding: 0 12px 0 8.5px;
      font-weight: 600;
    }}

    table {{
      width: 100%;
      border-collapse: collapse;
      margin: 12px 0;
      font-size: 8.5pt;
      direction: rtl;
      table-layout: auto;
      page-break-inside: auto;
      break-inside: auto;
    }}

    thead {{
      display: table-header-group;
    }}

    tr {{
      page-break-inside: avoid;
      break-inside: avoid;
    }}

    th, td {{
      border: 1px solid var(--border);
      padding: 6px 8px;
      text-align: right;
      vertical-align: top;
      overflow-wrap: anywhere;
      word-break: break-word;
    }}

    th {{
      background: #f1f5f9;
      font-weight: 700;
      color: #1e293b;
    }}

    tr:nth-child(even) {{
      background: #f8fafc;
    }}

    .callout {{
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 10px 14px;
      border-radius: 8px;
      margin: 12px 0;
      border-right: 4px solid;
      background: #f8fafc;
      break-inside: avoid;
    }}

    .callout-icon {{
      font-size: 15pt;
      line-height: 1;
    }}

    .callout-content {{
      flex: 1;
      font-size: 9.5pt;
    }}

    .callout-tip {{
      border-color: #10b981;
      background: #f0fdf4;
      color: #065f46;
    }}

    .callout-note {{
      border-color: #0284c7;
      background: #f0f9ff;
      color: #0c4a6e;
    }}

    .callout-important {{
      border-color: #f59e0b;
      background: #fffbeb;
      color: #92400e;
    }}

    .callout-warning {{
      border-color: #ef4444;
      background: #fef2f2;
      color: #991b1b;
    }}

    .callout-caution {{
      border-color: #dc2626;
      background: #fef2f2;
      color: #991b1b;
    }}

    .img-wrap {{
      text-align: center;
      margin: 12px auto;
      page-break-inside: avoid;
      break-inside: avoid;
    }}

    .img-caption {{
      font-size: 8.5pt;
      color: var(--text-muted);
      margin-top: 5px;
      font-weight: 600;
    }}

    .embedded-img {{
      max-width: 100%;
      max-height: 170mm;
      width: auto;
      height: auto;
      object-fit: contain;
      display: block;
      margin: 0 auto;
      border-radius: 8px;
      border: 1px solid var(--border);
      box-shadow: 0 3px 10px rgba(15, 23, 42, 0.08);
      page-break-inside: avoid;
      break-inside: avoid;
    }}

    .embedded-img.phone-shot {{
      max-height: 145mm;
      max-width: 72mm;
      border: 2px solid #334155;
      border-radius: 14px;
    }}

    hr {{
      border: none;
      border-top: 1px solid var(--border);
      margin: 20px 0;
    }}

    {pygments_css}
  </style>
</head>
<body>
  <div class="content-container">
    {body_html}
  </div>
</body>
</html>"""


def encode_relative_pdf_links(html_text: str) -> str:
    """Rewrites relative .pdf hrefs into a sentinel URL so Chromium does not convert them into machine-specific file:/// paths."""
    def replace_href(match):
        prefix, href, suffix = match.group(1), match.group(2), match.group(3)
        low = href.lower().strip()
        if low.startswith(("http://", "https://", "mailto:", "#", "data:")):
            return match.group(0)
        if ".pdf" not in low:
            return match.group(0)
        rel = href.strip().replace("\\", "/")
        while rel.startswith("./"):
            rel = rel[2:]
        return f'{prefix}{REL_PDF_SENTINEL}{quote(rel, safe="")}{suffix}'

    return re.sub(r'(<a\s+[^>]*href=")([^"]+)(")', replace_href, html_text, flags=re.IGNORECASE)


def rewrite_pdf_relative_links(pdf_path: Path) -> int:
    """Converts sentinel URIs inside a generated PDF into ISO 32000-1 relative /GoToR actions."""
    reader = PdfReader(str(pdf_path))
    writer = PdfWriter()
    writer.append(reader)

    converted = 0
    for page in writer.pages:
        annots = page.get("/Annots")
        if not annots:
            continue
        for annot_ref in annots:
            annot = annot_ref.get_object()
            action = annot.get("/A")
            if not action:
                continue
            action_obj = action.get_object()
            if action_obj.get("/S") != "/URI":
                continue
            uri = str(action_obj.get("/URI", ""))
            if uri.startswith(REL_PDF_SENTINEL):
                parsed = urlparse(uri)
                rel_path = unquote(parse_qs(parsed.query).get("p", [""])[0])
                if not rel_path:
                    continue
                filespec = DictionaryObject({
                    NameObject("/Type"): NameObject("/Filespec"),
                    NameObject("/F"): TextStringObject(rel_path),
                    NameObject("/UF"): TextStringObject(rel_path),
                })
                annot[NameObject("/A")] = DictionaryObject({
                    NameObject("/Type"): NameObject("/Action"),
                    NameObject("/S"): NameObject("/GoToR"),
                    NameObject("/F"): filespec,
                    NameObject("/D"): ArrayObject([NumberObject(0), NameObject("/Fit")]),
                    NameObject("/NewWindow"): BooleanObject(True),
                })
                converted += 1
            elif uri.lower().startswith("file:///"):
                print(f"  [WARN] Machine-specific file:/// URI detected in {pdf_path.name}: {uri}")

    if converted > 0:
        tmp_out = pdf_path.with_suffix(".relfixed.pdf")
        with open(tmp_out, "wb") as f:
            writer.write(f)
        shutil.move(str(tmp_out), str(pdf_path))
    return converted


def convert_markdown_to_pdf(page, md_file: Path, output_pdf: Path, learning_dir: Path):
    output_pdf.parent.mkdir(parents=True, exist_ok=True)
    md_text = md_file.read_text(encoding="utf-8", errors="ignore")
    title_match = re.search(r'^#\s+(.+)$', md_text, re.MULTILINE)
    title = title_match.group(1).strip() if title_match else md_file.stem

    md = MarkdownIt("commonmark", {"html": True, "highlight": highlight_code}).enable("table")
    html_body = md.render(md_text)
    html_body = transform_callouts(html_body)
    html_body = embed_images(html_body, md_file.parent, learning_dir)
    html_body = encode_relative_pdf_links(html_body)

    full_html = build_html_document(title=title, body_html=html_body)

    temp_html = output_pdf.with_suffix(".temp.html")
    temp_html.write_text(full_html, encoding="utf-8")

    try:
        page.goto(temp_html.as_uri(), wait_until="networkidle")
        page.pdf(
            path=str(output_pdf),
            format="A4",
            print_background=True,
            margin={"top": "14mm", "bottom": "14mm", "left": "12mm", "right": "12mm"}
        )
        rel_count = rewrite_pdf_relative_links(output_pdf)
        rel_note = f", {rel_count} relative PDF links" if rel_count else ""
        print(f"  [MD -> PDF] {output_pdf.name} ({output_pdf.stat().st_size / 1024:.1f} KB{rel_note})")
    except PermissionError:
        print(f"  [LOCKED] {output_pdf.name} is currently open in another program (e.g. PDF reader) - skipped overwrite.")
    finally:
        if temp_html.exists():
            temp_html.unlink()


def convert_html_to_pdf(page, html_file: Path, output_pdf: Path):
    output_pdf.parent.mkdir(parents=True, exist_ok=True)
    html_text = html_file.read_text(encoding="utf-8", errors="ignore")

    # Inject print-friendly styling if missing
    print_override = """
    <style>
      @page {
        size: A4 landscape;
        margin: 10mm;
      }
      body {
        margin: 10px !important;
        background: #ffffff !important;
        font-size: 10pt !important;
      }
      .card {
        box-shadow: none !important;
        break-inside: avoid !important;
        page-break-inside: avoid !important;
        margin-bottom: 18px !important;
      }
      pre {
        white-space: pre-wrap !important;
        word-break: break-word !important;
      }
    </style>
    """
    if "</head>" in html_text:
        html_text = html_text.replace("</head>", f"{print_override}</head>")
    else:
        html_text = f"{print_override}\n{html_text}"

    temp_html = output_pdf.with_suffix(".temp.html")
    temp_html.write_text(html_text, encoding="utf-8")

    try:
        page.goto(temp_html.as_uri(), wait_until="networkidle")
        page.pdf(
            path=str(output_pdf),
            format="A4",
            landscape=True,
            print_background=True,
            margin={"top": "10mm", "bottom": "10mm", "left": "10mm", "right": "10mm"}
        )
    finally:
        if temp_html.exists():
            temp_html.unlink()

    print(f"  [HTML -> PDF] {output_pdf.name} ({output_pdf.stat().st_size / 1024:.1f} KB)")


def convert_mmd_to_pdf(page, mmd_file: Path, output_pdf: Path, learning_dir: Path):
    output_pdf.parent.mkdir(parents=True, exist_ok=True)
    mmd_code = mmd_file.read_text(encoding="utf-8", errors="ignore")
    stem = mmd_file.stem
    low = stem.lower()
    if "flow" in low:
        title = "תרשים זרימת מסכים (Screen Flow Diagram)"
    elif "erd" in low:
        title = "תרשים קשרי ישויות (Entity-Relationship Diagram — ERD)"
    else:
        title = "תרשים מחלקות (UML Class Diagram)"

    # Check for corresponding rendered PNG
    png_path = learning_dir / "Images" / f"{stem}.png"
    img_html = ""
    if png_path.exists():
        with open(png_path, "rb") as f:
            b64 = base64.b64encode(f.read()).decode("utf-8")
        img_html = f'<div class="img-wrap"><img src="data:image/png;base64,{b64}" class="embedded-img" alt="{title}"></div>'

    body_html = f"""
    <div class="doc-header">
      <h1>{title}</h1>
      <div class="doc-subtitle">פרויקט .NET MAUI ו-SQLite — מפרט ותרשים מבנה נתונים</div>
    </div>
    <h2>1. תצוגה ויזואלית (Rendered Diagram)</h2>
    {img_html}
    <h2>2. קוד מקור Mermaid</h2>
    <div class="code-block" dir="ltr">
      <div class="code-header"><span class="code-lang">MERMAID</span></div>
      <pre class="highlight"><code>{mmd_code}</code></pre>
    </div>
    """

    full_html = build_html_document(title=title, body_html=body_html)
    temp_html = output_pdf.with_suffix(".temp.html")
    temp_html.write_text(full_html, encoding="utf-8")

    try:
        page.goto(temp_html.as_uri(), wait_until="networkidle")
        page.pdf(
            path=str(output_pdf),
            format="A4",
            print_background=True,
            margin={"top": "14mm", "bottom": "14mm", "left": "12mm", "right": "12mm"}
        )
    finally:
        if temp_html.exists():
            temp_html.unlink()

    print(f"  [MMD -> PDF] {output_pdf.name} ({output_pdf.stat().st_size / 1024:.1f} KB)")


def safe_copy(src: Path, dst: Path):
    if not src.exists():
        return
    dst.parent.mkdir(parents=True, exist_ok=True)
    try:
        shutil.copy2(src, dst)
    except Exception as e:
        print(f"  [LOCKED] Could not copy {src.name} to {dst.name}: {e}")


def main():
    repo_root = Path.cwd().resolve()
    learning_dir = repo_root / "Learning"
    guides_dir = learning_dir / "Guides"
    diagrams_dir = learning_dir / "Diagrams"

    maui_docs = repo_root / "PlayWithSQLITE" / "Docs"
    root_docs = repo_root / "Docs"

    targets = [maui_docs, root_docs]

    print("=================================================================")
    print("  MAUI Documentation & Guides to PDF Export Pipeline")
    print(f"  Source Guides:   {guides_dir}")
    print(f"  Source Diagrams: {diagrams_dir}")
    print(f"  Target 1 (MAUI): {maui_docs}")
    print(f"  Target 2 (Root): {root_docs}")
    print("=================================================================\n")

    # Collect files
    guide_files = sorted(list(guides_dir.glob("*.md"))) if guides_dir.exists() else []
    diagram_md_files = sorted(list(diagrams_dir.glob("*.md"))) if diagrams_dir.exists() else []
    diagram_html_files = sorted(list(diagrams_dir.glob("*.html"))) if diagrams_dir.exists() else []
    diagram_mmd_files = sorted(list(diagrams_dir.glob("*.mmd"))) if diagrams_dir.exists() else []

    with sync_playwright() as pw:
        browser = pw.chromium.launch()
        page = browser.new_page()

        print("[1/3] Converting Guides...")
        for g_file in guide_files:
            is_assignment = ("assignment" in g_file.name.lower()) or (g_file.name.startswith("00-"))
            if is_assignment:
                maui_pdf = maui_docs / "Assignment" / f"{g_file.stem}.pdf"
                root_pdf = root_docs / "Assignment" / f"{g_file.stem}.pdf"
            else:
                maui_pdf = maui_docs / f"{g_file.stem}.pdf"
                root_pdf = root_docs / f"{g_file.stem}.pdf"

            tmp_pdf = root_docs / f"_{g_file.stem}.tmp.pdf"
            convert_markdown_to_pdf(page, g_file, tmp_pdf, learning_dir)
            safe_copy(tmp_pdf, root_pdf)
            safe_copy(tmp_pdf, maui_pdf)
            if tmp_pdf.exists():
                tmp_pdf.unlink()

        print("\n[2/3] Converting Diagram Markdown & HTML files...")
        for d_md in diagram_md_files:
            maui_pdf = maui_docs / f"{d_md.stem}.pdf"
            maui_sub_pdf = maui_docs / "Diagrams" / f"{d_md.stem}.pdf"
            root_pdf = root_docs / f"{d_md.stem}.pdf"
            root_sub_pdf = root_docs / "Diagrams" / f"{d_md.stem}.pdf"

            tmp_pdf = root_docs / f"_{d_md.stem}.tmp.pdf"
            convert_markdown_to_pdf(page, d_md, tmp_pdf, learning_dir)
            safe_copy(tmp_pdf, root_pdf)
            safe_copy(tmp_pdf, root_sub_pdf)
            safe_copy(tmp_pdf, maui_pdf)
            safe_copy(tmp_pdf, maui_sub_pdf)
            if tmp_pdf.exists():
                tmp_pdf.unlink()

        for d_html in diagram_html_files:
            if "architecture-preview" in d_html.stem.lower():
                continue
            maui_pdf = maui_docs / f"{d_html.stem}.pdf"
            maui_sub_pdf = maui_docs / "Diagrams" / f"{d_html.stem}.pdf"
            root_pdf = root_docs / f"{d_html.stem}.pdf"
            root_sub_pdf = root_docs / "Diagrams" / f"{d_html.stem}.pdf"

            tmp_pdf = root_docs / f"_{d_html.stem}.tmp.pdf"
            convert_html_to_pdf(page, d_html, tmp_pdf)
            safe_copy(tmp_pdf, root_pdf)
            safe_copy(tmp_pdf, root_sub_pdf)
            safe_copy(tmp_pdf, maui_pdf)
            safe_copy(tmp_pdf, maui_sub_pdf)
            if tmp_pdf.exists():
                tmp_pdf.unlink()

        print("\n[3/3] Converting Mermaid Diagram files...")
        for d_mmd in diagram_mmd_files:
            maui_pdf = maui_docs / f"{d_mmd.stem}.pdf"
            maui_sub_pdf = maui_docs / "Diagrams" / f"{d_mmd.stem}.pdf"
            root_pdf = root_docs / f"{d_mmd.stem}.pdf"
            root_sub_pdf = root_docs / "Diagrams" / f"{d_mmd.stem}.pdf"

            tmp_pdf = root_docs / f"_{d_mmd.stem}.tmp.pdf"
            convert_mmd_to_pdf(page, d_mmd, tmp_pdf, learning_dir)
            safe_copy(tmp_pdf, root_pdf)
            safe_copy(tmp_pdf, root_sub_pdf)
            safe_copy(tmp_pdf, maui_pdf)
            safe_copy(tmp_pdf, maui_sub_pdf)
            if tmp_pdf.exists():
                tmp_pdf.unlink()

        browser.close()

    print("\n[SUCCESS] All Guides and Diagrams successfully converted to PDF!")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
