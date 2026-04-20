

- The API user (merchant) should be able to receive a response from the gateway
    - 200 Authorized - the payment was authorized by the call to the acquiring bank
    - 200 Declined - the payment was declined by the call to the acquiring bank
    - 400 Rejected - No payment could be created as invalid information was supplied to the payment gateway and therefore it has rejected the request without calling the acquiring bank
- The API user (merchant) should be able to retrieve details of previous payments


 

