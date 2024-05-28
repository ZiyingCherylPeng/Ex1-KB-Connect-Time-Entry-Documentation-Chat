
import pdfplumber
from langchain import LangChain, Chain
from PyMuPDF import fitz

# 读取 PDF 文件中的文本
def read_pdf_text(file_path):
    doc = fitz.open(file_path)
    text = ""
    for page in doc:
        text += page.get_text()
    return text

# 提取 PDF 文件中的表格
def read_pdf_tables(file_path):
    tables = []
    with pdfplumber.open(file_path) as pdf:
        for page in pdf.pages:
            extracted_tables = page.extract_tables()
            for table in extracted_tables:
                tables.append(table)
    return tables

# 提取 PDF 文件中的图像
def read_pdf_images(file_path):
    doc = fitz.open(file_path)
    images = []
    for page_number in range(len(doc)):
        page = doc.load_page(page_number)
        image_list = page.get_images(full=True)
        for img_index, img in enumerate(image_list):
            xref = img[0]
            base_image = doc.extract_image(xref)
            image_bytes = base_image["image"]
            images.append(image_bytes)
    return images

# 示例 PDF 文件路径
pdf_file_path = "C:/Users/zp111454/Desktop/ex1-KB/Connect Time Entry  Time Card/test2(contain image).pdf"

# 提取 PDF 文件中的文本、表格和图像
pdf_text = read_pdf_text(pdf_file_path)
pdf_tables = read_pdf_tables(pdf_file_path)
pdf_images = read_pdf_images(pdf_file_path)

# 使用 LangChain 处理提取的内容
lang_chain = LangChain()
chain = Chain(steps=[
    {"action": "preprocess_text", "text": pdf_text},
    {"action": "process_tables", "tables": pdf_tables},
    {"action": "analyze_images", "images": pdf_images}
])

# 执行链式处理
response = lang_chain.run(chain)
print(response)
