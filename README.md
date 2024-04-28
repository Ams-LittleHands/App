<h2>Thư mục</h2>
<ul>
  <li>Application: Để code liên quan tới ứng dụng.</li>
  <li> 
    Server
    <ul>
     <li>Hand_Sign_Classification: Code dùng để phân tách và phân loại video về ngôn ngữ khiếm thính.</li>
     <li>Vietnamese_SignLanugage_Conversion: Code dùng để chuyển đổi giữa ngôn ngữ khiếm thính và tiếng việt.</li>
    </ul>
  </li>
  <li>Models: Để weights của các mô hình.</li>
  <li>Others: Để prompt dùng cho việc chuyển đổi giữa ngôn ngữ khiếm thính và tiếng việt.</li>
  <li>Documents: Để các tài liệu liên quan.</li>
</ul>
<h2>Chạy code</h2>
<ul>
   <li> Tải weights cho mô hình phân loại video về ngôn ngữ khiếm thính:
      <ul>
       <li>Nếu chưa có gdown: pip install gdown</li>
       <li>cd Models</li>
       <li>gdown 1S9OOxt39vxk_ncZg9GAhUrx8FwFALcmm</li>
      </ul>
   </li>
   <li> Tải source mmdeploy:
      <ul>
       <li>cd Server/Hand_Sign_Classification</li>
       <li>git clone https://github.com/open-mmlab/mmdeploy.git</li>
      </ul>
   </li>
   <li> Tải source PhoNLP:
      <ul>
       <li>cd Server/Vietnamese_SignLanugage_Conversion</li>
       <li>git clone https://github.com/VinAIResearch/PhoNLP.git</li>
      </ul>
   </li>
  <li> Chạy api phân loại video về ngôn ngữ khiếm thính:
      <ul>
       <li>cd Server/Vietnamese_SignLanugage_Conversion</li>
       <li>gunicorn fastAPI:app --bind 0.0.0.0:9091 --worker-class uvicorn.workers.UvicornWorker --timeout 300</li>
      </ul>
   </li>
   <li> Chạy api chuyển đổi ngôn ngữ khiếm thính sang tiếng việt:
      <ul>
       <li>cd Server/Vietnamese_SignLanugage_Conversion</li>
       <li>python3 main.py</li>
      </ul>
   </li>
  <li> Chạy app
</ul>

