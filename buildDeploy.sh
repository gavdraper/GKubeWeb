docker build -t gkubeweb:V1 -f Dockerfile .
alias k=kubectl
k delete deployment gkubeweb
k apply -f deployment.yaml