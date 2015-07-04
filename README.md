#Token generator for Wowza Token Authentication


REQUIRE

  Java 6 or 7, Maven 2 or 3


BUILD

  To build the generator go to /wowza-token-auth-generator-java/ and run the following command:
```  
mvn clean install
```  
  Upon success of the build, you will find the jar file (**token-auth-generator-1.2.jar**) in the folder named 'target'


USAGE
```
java -jar token-auth-generator-1.2.jar (encrypt | decrypt) (<primary_key> | <backup_key>) "<security_parameters>"
```


SECURITY PARAMETERS

 * expire
   * Number of seconds since Unix time (Epoch time) 
   * UTC based 
   * Must not be earlier than current time


 * ref_allow
  *  Referrer domain (e.g. domain.com) or path (e.g. domain.com/video/)
  *  Allow multiple referrers separated by comma (,) without space(s)
  *  Wildcard (*) allowed only at the beginning of a referrer, e.g. *.domain.com
  *  Do not append space at the start & end of a referrer
  *  Domain must fullfill RFC 3490
  *  Path must fullfill RFC 2396
  *  Should not include port (e.g. domain.com:3000/video)
  *  Should not include protocol portion  (e.g. http://domain.com)

 * ref_deny
   * Same rules as in ref_allow
   * Normally ref_allow  & ref_deny are not to be used together, but if this happened ref_allow will take precedence over ref_deny.


ALLOW BLANK / MISSING REFERRER

  Both "ref_allow" & "ref_deny" could be configured to allow/deny blank or missing referrer during Token Auth validation.

The following configuration allow blank or missing referrer:
  * ref_allow=allow.com,
  * ref_allow=allow.com,MISSING
  * ref_deny=deny.com

The following configuration deny blank or missing referrer:
  * ref_allow=allow.com
  * ref_deny=deny.com,
  * ref_deny=deny.com,MISSING
  * Normally ref_allow  & ref_deny are not to be used together, but if this happened ref_allow will take precedence over ref_deny.


TO GENERATE TOKEN

```
java -jar token-auth-generator-1.2.jar encrypt samplekey "expire=1598832000&ref_allow=*.trusted.com&ref_deny=denied.com"
```
Sample Output:
```
token=110ea31ac69c09a2db0bdd74238843631cdab498ff7e6e75cbd99cc4d05426ab679a57015d4e48438c97b921652daec62de3829f8ff437e27449cfdfc2f1e5d9fc47f14e91a51ea7
```
Then append the result to the end of the streaming CDN URL as in the following example:
```    
rtmp://12345.r.cdnsun.net/_definst_/live?token=110ea31ac69c09a2db0bdd74238843631cdab498ff7e6e75cbd99cc4d05426ab679a57015d4e48438c97b921652daec62de3829f8ff437e27449cfdfc2f1e5d9fc47f14e91a51ea7
```

TO DECRYPT TOKEN (for debugging purposes)

```
java -jar token-auth-generator-1.2.jar decrypt samplekey 110ea31ac69c09a2db0bdd74238843631cdab498ff7e6e75cbd99cc4d05426ab679a57015d4e48438c97b921652daec62de3829f8ff437e27449cfdfc2f1e5d9fc47f14e91a51ea7
```
Sample Output:
```
security parameters=expire=1598832000&ref_allow=*.trusted.com&ref_deny=denied.com
```
